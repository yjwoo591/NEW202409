using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace PC1MAINAITradingSystem.Core.Security
{
    public partial class SecurityManager
    {
        private readonly ConcurrentDictionary<string, HashSet<Permission>> _userPermissions = new();
        private readonly ConcurrentDictionary<string, HashSet<string>> _userRoles = new();
        private readonly ConcurrentDictionary<string, HashSet<Permission>> _rolePermissions = new();

        public async Task<bool> ValidatePermissions(DatabasePermissions permission)
        {
            try
            {
                if (!_isInitialized || !IsAuthenticated)
                {
                    return false;
                }

                // 시스템 관리자는 모든 권한 보유
                if (await IsSystemAdmin(_currentUser))
                {
                    return true;
                }

                var requiredPermission = ConvertToPermission(permission);
                return await HasPermission(_currentUser, requiredPermission);
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Permission validation failed for {permission}", ex);
                return false;
            }
        }

        public async Task<bool> GrantPermission(string username, Permission permission)
        {
            try
            {
                if (!_isInitialized || string.IsNullOrEmpty(username))
                {
                    return false;
                }

                // 권한 부여 권한 확인
                if (!await HasPermission(_currentUser, Permission.ManagePermissions))
                {
                    await _logger.LogWarning($"User {_currentUser} attempted to grant permission without authorization");
                    return false;
                }

                _userPermissions.AddOrUpdate(
                    username,
                    new HashSet<Permission> { permission },
                    (_, permissions) =>
                    {
                        permissions.Add(permission);
                        return permissions;
                    });

                await _logger.LogInfo($"Granted permission {permission} to user {username}");
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Failed to grant permission {permission} to user {username}", ex);
                return false;
            }
        }

        public async Task<bool> RevokePermission(string username, Permission permission)
        {
            try
            {
                if (!_isInitialized || string.IsNullOrEmpty(username))
                {
                    return false;
                }

                // 권한 해제 권한 확인
                if (!await HasPermission(_currentUser, Permission.ManagePermissions))
                {
                    await _logger.LogWarning($"User {_currentUser} attempted to revoke permission without authorization");
                    return false;
                }

                if (_userPermissions.TryGetValue(username, out var permissions))
                {
                    permissions.Remove(permission);
                    await _logger.LogInfo($"Revoked permission {permission} from user {username}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Failed to revoke permission {permission} from user {username}", ex);
                return false;
            }
        }

        public async Task<bool> AssignRole(string username, string role)
        {
            try
            {
                if (!_isInitialized || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(role))
                {
                    return false;
                }

                // 역할 할당 권한 확인
                if (!await HasPermission(_currentUser, Permission.ManageRoles))
                {
                    await _logger.LogWarning($"User {_currentUser} attempted to assign role without authorization");
                    return false;
                }

                _userRoles.AddOrUpdate(
                    username,
                    new HashSet<string> { role },
                    (_, roles) =>
                    {
                        roles.Add(role);
                        return roles;
                    });

                await _logger.LogInfo($"Assigned role {role} to user {username}");
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Failed to assign role {role} to user {username}", ex);
                return false;
            }
        }

        public async Task<bool> RemoveRole(string username, string role)
        {
            try
            {
                if (!_isInitialized || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(role))
                {
                    return false;
                }

                // 역할 제거 권한 확인
                if (!await HasPermission(_currentUser, Permission.ManageRoles))
                {
                    await _logger.LogWarning($"User {_currentUser} attempted to remove role without authorization");
                    return false;
                }

                if (_userRoles.TryGetValue(username, out var roles))
                {
                    roles.Remove(role);
                    await _logger.LogInfo($"Removed role {role} from user {username}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Failed to remove role {role} from user {username}", ex);
                return false;
            }
        }

        private async Task<bool> HasPermission(string username, Permission permission)
        {
            // 직접 할당된 권한 확인
            if (_userPermissions.TryGetValue(username, out var directPermissions) &&
                directPermissions.Contains(permission))
            {
                return true;
            }

            // 역할을 통한 권한 확인
            if (_userRoles.TryGetValue(username, out var roles))
            {
                foreach (var role in roles)
                {
                    if (_rolePermissions.TryGetValue(role, out var rolePermissions) &&
                        rolePermissions.Contains(permission))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private async Task<bool> IsSystemAdmin(string username)
        {
            return _userRoles.TryGetValue(username, out var roles) &&
                   roles.Contains("SystemAdmin");
        }

        private Permission ConvertToPermission(DatabasePermissions dbPermission)
        {
            return dbPermission switch
            {
                DatabasePermissions.Read => Permission.DatabaseRead,
                DatabasePermissions.Write => Permission.DatabaseWrite,
                DatabasePermissions.Execute => Permission.DatabaseExecute,
                DatabasePermissions.Backup => Permission.DatabaseBackup,
                DatabasePermissions.Restore => Permission.DatabaseRestore,
                DatabasePermissions.Create => Permission.DatabaseCreate,
                DatabasePermissions.Alter => Permission.DatabaseAlter,
                DatabasePermissions.Drop => Permission.DatabaseDrop,
                DatabasePermissions.Admin => Permission.DatabaseAdmin,
                _ => throw new ArgumentException($"Unknown database permission: {dbPermission}")
            };
        }
    }

    public enum Permission
    {
        // 데이터베이스 권한
        DatabaseRead,
        DatabaseWrite,
        DatabaseExecute,
        DatabaseBackup,
        DatabaseRestore,
        DatabaseCreate,
        DatabaseAlter,
        DatabaseDrop,
        DatabaseAdmin,

        // ERD 권한
        ERDView,
        ERDEdit,
        ERDCreate,
        ERDDelete,
        ERDValidate,
        ERDCompare,
        ERDExport,

        // 시스템 권한
        ManageUsers,
        ManageRoles,
        ManagePermissions,
        ViewAuditLog,
        SystemConfig,
        SystemAdmin
    }

    public class PermissionGroup
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public HashSet<Permission> Permissions { get; set; } = new();
    }

    public class Role
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public HashSet<Permission> Permissions { get; set; } = new();
        public HashSet<string> InheritedRoles { get; set; } = new();
    }
}