using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using PC1MAINAITradingSystem.Models.ERD;
using System.Security;

namespace PC1MAINAITradingSystem.Core.ERD.Base
{
    public partial class ERDCoreBase
    {
        private readonly ILogger _logger;
        private readonly IConfigManager _configManager;
        private readonly SecurityManager _securityManager;
        private readonly ERDConfig _config;

        private bool _isInitialized;
        private ERDModel _currentERD;
        private string _currentPath;

        public ERDCoreBase(
            ILogger logger,
            IConfigManager configManager,
            SecurityManager securityManager,
            ERDConfig config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
            _securityManager = securityManager ?? throw new ArgumentNullException(nameof(securityManager));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<bool> Initialize()
        {
            try
            {
                if (_isInitialized)
                {
                    await _logger.LogWarning("ERD Core is already initialized");
                    return true;
                }

                // 설정 검증
                if (!ValidateConfiguration())
                {
                    return false;
                }

                // 기본 디렉토리 구조 생성
                await EnsureDirectoryStructure();

                // 기본 ERD 파일 검증
                await ValidateERDFiles();

                _isInitialized = true;
                await _logger.LogInfo("ERD Core initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogError("Failed to initialize ERD Core", ex);
                return false;
            }
        }

        public ERDModel CurrentERD => _currentERD;
        public string CurrentPath => _currentPath;
        public bool IsInitialized => _isInitialized;

        private bool ValidateConfiguration()
        {
            if (string.IsNullOrWhiteSpace(_config.BaseERDPath))
            {
                _logger.LogError("Base ERD path is not specified").Wait();
                return false;
            }

            if (string.IsNullOrWhiteSpace(_config.BackupPath))
            {
                _logger.LogError("Backup path is not specified").Wait();
                return false;
            }

            return true;
        }

        private async Task EnsureDirectoryStructure()
        {
            var directories = new[]
            {
                _config.BaseERDPath,
                _config.BackupPath,
                _config.TempPath,
                _config.ExportPath
            };

            foreach (var directory in directories)
            {
                if (!System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                    await _logger.LogInfo($"Created directory: {directory}");
                }
            }
        }

        private async Task ValidateERDFiles()
        {
            var requiredFiles = new[]
            {
                System.IO.Path.Combine(_config.BaseERDPath, "PC1ERD.mermaid"),
                System.IO.Path.Combine(_config.BaseERDPath, "PC1ERDex.mermaid")
            };

            foreach (var file in requiredFiles)
            {
                if (!System.IO.File.Exists(file))
                {
                    await CreateDefaultERDFile(file);
                    await _logger.LogInfo($"Created default ERD file: {file}");
                }
            }
        }

        private async Task CreateDefaultERDFile(string path)
        {
            var defaultContent = @"erDiagram
    SYSTEM {
        int Id PK
        string Name
        datetime CreatedAt
        datetime ModifiedAt
        string Status
    }";

            await System.IO.File.WriteAllTextAsync(path, defaultContent);
        }
    }

    public class ERDConfig
    {
        public string BaseERDPath { get; set; }
        public string BackupPath { get; set; }
        public string TempPath { get; set; }
        public string ExportPath { get; set; }
        public bool AutoBackup { get; set; } = true;
        public int BackupRetentionDays { get; set; } = 30;
        public bool ValidateOnLoad { get; set; } = true;
        public bool EnableVersionControl { get; set; } = true;
        public string DefaultERDTemplate { get; set; }
        public Dictionary<string, string> CustomSettings { get; set; } = new();
    }
}