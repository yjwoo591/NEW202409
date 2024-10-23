using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using PC1MAINAITradingSystem.Models.ERD;
using PC1MAINAITradingSystem.Core.ERD.Generator;
using PC1MAINAITradingSystem.Core.ERD.Parser;
using System.Security;

namespace PC1MAINAITradingSystem.Core.ERD.Base
{
    public partial class ERDCoreBase
    {
        public async Task<OperationResult<ERDModel>> LoadERD(string path)
        {
            try
            {
                if (!_isInitialized)
                {
                    throw new InvalidOperationException("ERD Core is not initialized");
                }

                // 권한 검증
                if (!await _securityManager.ValidatePermissions(Permission.ERDView))
                {
                    return new OperationResult<ERDModel>
                    {
                        Success = false,
                        Message = "Insufficient permissions to load ERD"
                    };
                }

                // 파일 존재 여부 확인
                if (!File.Exists(path))
                {
                    return new OperationResult<ERDModel>
                    {
                        Success = false,
                        Message = $"ERD file not found: {path}"
                    };
                }

                // 파일 내용 읽기
                var content = await File.ReadAllTextAsync(path);

                // ERD 파싱
                var parser = new ERDParser(_logger);
                var (model, errors) = await parser.ParseERD(content);

                if (errors.Any())
                {
                    return new OperationResult<ERDModel>
                    {
                        Success = false,
                        Message = "ERD parsing failed",
                        Data = model,
                        Errors = errors
                    };
                }

                // 유효성 검사
                if (_config.ValidateOnLoad)
                {
                    var validationResult = await ValidateERD(model);
                    if (!validationResult.IsValid)
                    {
                        return new OperationResult<ERDModel>
                        {
                            Success = false,
                            Message = "ERD validation failed",
                            Data = model,
                            Errors = validationResult.Errors
                        };
                    }
                }

                // 현재 ERD 업데이트
                _currentERD = model;
                _currentPath = path;

                await _logger.LogInfo($"ERD loaded successfully from {path}");
                return new OperationResult<ERDModel>
                {
                    Success = true,
                    Data = model
                };
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Failed to load ERD from {path}", ex);
                return new OperationResult<ERDModel>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<OperationResult> SaveERD(ERDModel model, string path = null)
        {
            try
            {
                if (!_isInitialized)
                {
                    throw new InvalidOperationException("ERD Core is not initialized");
                }

                // 권한 검증
                if (!await _securityManager.ValidatePermissions(Permission.ERDEdit))
                {
                    return new OperationResult
                    {
                        Success = false,
                        Message = "Insufficient permissions to save ERD"
                    };
                }

                // 저장 경로 결정
                path ??= _currentPath;
                if (string.IsNullOrEmpty(path))
                {
                    return new OperationResult
                    {
                        Success = false,
                        Message = "Save path not specified"
                    };
                }

                // 백업 생성
                if (_config.AutoBackup)
                {
                    await CreateBackup(path);
                }

                // ERD 검증
                var validationResult = await ValidateERD(model);
                if (!validationResult.IsValid)
                {
                    return new OperationResult
                    {
                        Success = false,
                        Message = "ERD validation failed",
                        Errors = validationResult.Errors
                    };
                }

                // ERD를 Mermaid 형식으로 변환
                var generator = new ERDGenerator(_logger);
                var (content, warnings) = await generator.GenerateMermaidERD(model);

                // 파일 저장
                await File.WriteAllTextAsync(path, content);

                // 현재 ERD 업데이트
                _currentERD = model;
                _currentPath = path;

                await _logger.LogInfo($"ERD saved successfully to {path}");
                return new OperationResult
                {
                    Success = true,
                    Warnings = warnings
                };
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Failed to save ERD to {path}", ex);
                return new OperationResult
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<OperationResult> CreateBackup(string originalPath)
        {
            try
            {
                if (!File.Exists(originalPath))
                {
                    return new OperationResult
                    {
                        Success = false,
                        Message = "Original file not found"
                    };
                }

                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var backupFileName = $"{Path.GetFileNameWithoutExtension(originalPath)}_{timestamp}.bak";
                var backupPath = Path.Combine(_config.BackupPath, backupFileName);

                File.Copy(originalPath, backupPath);

                // 오래된 백업 정리
                await CleanupOldBackups();

                await _logger.LogInfo($"Backup created: {backupPath}");
                return new OperationResult { Success = true };
            }
            catch (Exception ex)
            {
                await _logger.LogError("Backup creation failed", ex);
                return new OperationResult
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<OperationResult> RestoreFromBackup(string backupPath)
        {
            try
            {
                if (!_isInitialized)
                {
                    throw new InvalidOperationException("ERD Core is not initialized");
                }

                // 권한 검증
                if (!await _securityManager.ValidatePermissions(Permission.ERDEdit))
                {
                    return new OperationResult
                    {
                        Success = false,
                        Message = "Insufficient permissions to restore ERD"
                    };
                }

                if (!File.Exists(backupPath))
                {
                    return new OperationResult
                    {
                        Success = false,
                        Message = "Backup file not found"
                    };
                }

                // 현재 파일 백업
                if (_currentPath != null)
                {
                    await CreateBackup(_currentPath);
                }

                // 백업에서 복원
                File.Copy(backupPath, _currentPath, true);

                // ERD 다시 로드
                var loadResult = await LoadERD(_currentPath);
                if (!loadResult.Success)
                {
                    return new OperationResult
                    {
                        Success = false,
                        Message = "Failed to load restored ERD",
                        Errors = loadResult.Errors
                    };
                }

                await _logger.LogInfo($"ERD restored from backup: {backupPath}");
                return new OperationResult { Success = true };
            }
            catch (Exception ex)
            {
                await _logger.LogError("ERD restoration failed", ex);
                return new OperationResult
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        private async Task CleanupOldBackups()
        {
            try
            {
                var directory = new DirectoryInfo(_config.BackupPath);
                var files = directory.GetFiles("*.bak")
                    .OrderByDescending(f => f.CreationTime)
                    .Skip(_config.BackupRetentionDays);

                foreach (var file in files)
                {
                    file.Delete();
                    await _logger.LogInfo($"Deleted old backup: {file.Name}");
                }
            }
            catch (Exception ex)
            {
                await _logger.LogError("Failed to cleanup old backups", ex);
            }
        }
    }

    public class OperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    public class OperationResult<T> : OperationResult
    {
        public T Data { get; set; }
    }
}