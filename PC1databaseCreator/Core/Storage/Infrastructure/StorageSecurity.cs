using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PC1databaseCreator.Core.Storage.Models;

namespace PC1databaseCreator.Core.Storage.Infrastructure
{
    public class StorageSecurity
    {
        private readonly ILogger<StorageSecurity> _logger;
        private readonly WindowsIdentity _currentIdentity;

        public StorageSecurity(ILogger<StorageSecurity> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentIdentity = WindowsIdentity.GetCurrent();
        }

        public async Task<StorageResult> SetSecurityAsync(
            string path,
            SecurityConfiguration config,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await Task.Run(() =>
                {
                    var security = new DirectorySecurity();

                    // 기본 소유자 설정
                    security.SetOwner(_currentIdentity.User);

                    // 관리자 권한 설정
                    if (config.AdminAccess)
                    {
                        var adminRule = new FileSystemAccessRule(
                            new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
                            FileSystemRights.FullControl,
                            InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                            PropagationFlags.None,
                            AccessControlType.Allow);
                        security.AddAccessRule(adminRule);
                    }

                    // 시스템 권한 설정
                    var systemRule = new FileSystemAccessRule(
                        new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null),
                        FileSystemRights.FullControl,
                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                        PropagationFlags.None,
                        AccessControlType.Allow);
                    security.AddAccessRule(systemRule);

                    // 사용자 권한 설정
                    if (config.UserAccess)
                    {
                        var userRights = FileSystemRights.Read | FileSystemRights.Write |
                                       FileSystemRights.ListDirectory;
                        var userRule = new FileSystemAccessRule(
                            _currentIdentity.Name,
                            userRights,
                            InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                            PropagationFlags.None,
                            AccessControlType.Allow);
                        security.AddAccessRule(userRule);
                    }

                    Directory.SetAccessControl(path, security);
                }, cancellationToken);

                _logger.LogInformation("Successfully set security for path: {Path}", path);
                return StorageResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting security for path: {Path}", path);
                return StorageResult.Failure(
                    StorageErrorType.AccessDenied,
                    "Error setting security configuration",
                    ex);
            }
        }

        public async Task<StorageResult<SecurityInfo>> GetSecurityInfoAsync(
            string path,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await Task.Run(() =>
                {
                    var security = Directory.GetAccessControl(path);
                    var rules = security.GetAccessRules(true, true, typeof(SecurityIdentifier));

                    var info = new SecurityInfo
                    {
                        Path = path,
                        Owner = security.GetOwner(typeof(SecurityIdentifier)).ToString(),
                        IsAdminAccessGranted = HasAdminAccess(rules),
                        IsUserAccessGranted = HasUserAccess(rules),
                        CheckTime = DateTime.UtcNow
                    };

                    return StorageResult<SecurityInfo>.Success(info);
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting security info for path: {Path}", path);
                return StorageResult<SecurityInfo>.Failure(
                    StorageErrorType.AccessDenied,
                    "Error getting security information",
                    ex);
            }
        }

        private bool HasAdminAccess(AuthorizationRuleCollection rules)
        {
            var adminSid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
            return rules.Cast<FileSystemAccessRule>()
                .Any(rule => rule.IdentityReference.Value == adminSid.Value &&
                            rule.FileSystemRights.HasFlag(FileSystemRights.FullControl) &&
                            rule.AccessControlType == AccessControlType.Allow);
        }

        private bool HasUserAccess(AuthorizationRuleCollection rules)
        {
            return rules.Cast<FileSystemAccessRule>()
                .Any(rule => rule.IdentityReference.Value == _currentIdentity.User.Value &&
                            rule.FileSystemRights.HasFlag(FileSystemRights.Read | FileSystemRights.Write) &&
                            rule.AccessControlType == AccessControlType.Allow);
        }
    }

    public record SecurityConfiguration
    {
        public bool AdminAccess { get; init; } = true;
        public bool UserAccess { get; init; } = true;
        public bool InheritPermissions { get; init; } = true;
    }

    public record SecurityInfo
    {
        public string Path { get; init; }
        public string Owner { get; init; }
        public bool IsAdminAccessGranted { get; init; }
        public bool IsUserAccessGranted { get; init; }
        public DateTime CheckTime { get; init; }
    }
}