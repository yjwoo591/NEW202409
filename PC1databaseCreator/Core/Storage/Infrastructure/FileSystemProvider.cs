using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PC1databaseCreator.Core.Storage.Models;
using PC1databaseCreator.Core.Storage.Constants;

namespace PC1databaseCreator.Core.Storage.Infrastructure
{
    public class FileSystemProvider
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILogger<FileSystemProvider> _logger;
        private readonly string _rootPath;

        public FileSystemProvider(
            IFileSystem fileSystem,
            ILogger<FileSystemProvider> logger,
            string rootPath)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _rootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));

            InitializeFileSystem();
        }

        private void InitializeFileSystem()
        {
            // 기본 디렉토리 구조 생성
            foreach (var path in new[] {
                StorageConstants.Paths.FAST_ACCESS_FOLDER,
                StorageConstants.Paths.ARCHIVE_FOLDER,
                StorageConstants.Paths.TEMP_FOLDER
            })
            {
                var fullPath = _fileSystem.Path.Combine(_rootPath, path);
                if (!_fileSystem.Directory.Exists(fullPath))
                {
                    _fileSystem.Directory.CreateDirectory(fullPath);
                }
            }
        }

        public async Task<StorageResult<byte[]>> ReadFileAsync(string path, CancellationToken cancellationToken)
        {
            try
            {
                var fullPath = _fileSystem.Path.Combine(_rootPath, path);
                if (!_fileSystem.File.Exists(fullPath))
                {
                    return StorageResult<byte[]>.Failure(
                        StorageErrorType.PathNotFound,
                        $"File not found: {path}");
                }

                using var stream = _fileSystem.File.OpenRead(fullPath);
                var buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                _logger.LogDebug("Successfully read file: {Path}", path);
                return StorageResult<byte[]>.Success(buffer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file: {Path}", path);
                return StorageResult<byte[]>.Failure(
                    StorageErrorType.InvalidOperation,
                    "Error reading file",
                    ex);
            }
        }

        public async Task<StorageResult> WriteFileAsync(string path, byte[] data, CancellationToken cancellationToken)
        {
            try
            {
                var fullPath = _fileSystem.Path.Combine(_rootPath, path);
                var directory = _fileSystem.Path.GetDirectoryName(fullPath);

                if (!_fileSystem.Directory.Exists(directory))
                {
                    _fileSystem.Directory.CreateDirectory(directory);
                }

                // 공간 확인
                var driveInfo = _fileSystem.DriveInfo.GetDrives()
                    .First(d => fullPath.StartsWith(d.Name, StringComparison.OrdinalIgnoreCase));

                if (driveInfo.AvailableFreeSpace < data.Length)
                {
                    return StorageResult.Failure(
                        StorageErrorType.InsufficientSpace,
                        StorageConstants.ErrorMessages.INSUFFICIENT_SPACE);
                }

                await using var stream = _fileSystem.File.Create(fullPath);
                await stream.WriteAsync(data, 0, data.Length, cancellationToken);

                _logger.LogDebug("Successfully wrote file: {Path}", path);
                return StorageResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing file: {Path}", path);
                return StorageResult.Failure(
                    StorageErrorType.InvalidOperation,
                    "Error writing file",
                    ex);
            }
        }

        public StorageResult DeleteFile(string path)
        {
            try
            {
                var fullPath = _fileSystem.Path.Combine(_rootPath, path);
                if (_fileSystem.File.Exists(fullPath))
                {
                    _fileSystem.File.Delete(fullPath);
                    _logger.LogDebug("Successfully deleted file: {Path}", path);
                    return StorageResult.Success();
                }
                return StorageResult.Success($"File not found: {path}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {Path}", path);
                return StorageResult.Failure(
                    StorageErrorType.InvalidOperation,
                    "Error deleting file",
                    ex);
            }
        }

        public StorageResult<StorageMetrics> GetMetrics()
        {
            try
            {
                var drive = _fileSystem.DriveInfo.GetDrives()
                    .First(d => _rootPath.StartsWith(d.Name, StringComparison.OrdinalIgnoreCase));

                var metrics = new StorageMetrics
                {
                    TotalSpace = drive.TotalSize,
                    AvailableSpace = drive.AvailableFreeSpace,
                    UsedSpace = drive.TotalSize - drive.AvailableFreeSpace,
                    MeasurementTime = DateTime.UtcNow,
                    Status = GetStorageStatus(drive)
                };

                return StorageResult<StorageMetrics>.Success(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting storage metrics");
                return StorageResult<StorageMetrics>.Failure(
                    StorageErrorType.InvalidOperation,
                    "Error getting storage metrics",
                    ex);
            }
        }

        private static StorageStatus GetStorageStatus(IDriveInfo drive)
        {
            var usagePercentage = ((double)(drive.TotalSize - drive.AvailableFreeSpace) / drive.TotalSize) * 100;

            return usagePercentage switch
            {
                >= StorageConstants.Monitoring.CRITICAL_SPACE_THRESHOLD => StorageStatus.Critical,
                >= StorageConstants.Monitoring.WARNING_SPACE_THRESHOLD => StorageStatus.Warning,
                _ => StorageStatus.OK
            };
        }
    }
}