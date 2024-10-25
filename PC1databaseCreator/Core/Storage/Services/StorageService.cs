using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using PC1databaseCreator.Core.Storage.Interfaces;
using PC1databaseCreator.Core.Storage.Models;
using PC1databaseCreator.Core.Storage.Infrastructure;
using PC1databaseCreator.Core.Storage.Constants;

namespace PC1databaseCreator.Core.Storage.Services
{
    public class StorageService : IStorageService
    {
        private readonly ILogger<StorageService> _logger;
        private readonly ConcurrentDictionary<StorageType, FileSystemProvider> _providers;
        private readonly IStorageCache _cache;
        private readonly IStorageMonitor _monitor;
        private readonly StorageSecurity _security;

        public StorageService(
            IFileSystem fileSystem,
            IStorageCache cache,
            IStorageMonitor monitor,
            string rootPath)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            _rootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));

            // 루트 디렉토리 생성
            if (!_fileSystem.Directory.Exists(_rootPath))
            {
                _fileSystem.Directory.CreateDirectory(_rootPath);
            }
        }

        public async Task<bool> SaveDataAsync(string path, byte[] data, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
            if (data == null) throw new ArgumentNullException(nameof(data));

            try
            {
                string fullPath = _fileSystem.Path.Combine(_rootPath, path);
                string directory = _fileSystem.Path.GetDirectoryName(fullPath);

                if (!_fileSystem.Directory.Exists(directory))
                {
                    _fileSystem.Directory.CreateDirectory(directory);
                }

                await using var stream = _fileSystem.File.Create(fullPath);
                await stream.WriteAsync(data, 0, data.Length, cancellationToken);

                // 캐시 업데이트
                await _cache.SetAsync(path, data, TimeSpan.FromMinutes(30), cancellationToken);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<byte[]> LoadDataAsync(string path, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

            // 캐시 확인
            var cachedData = await _cache.GetAsync(path, cancellationToken);
            if (cachedData != null) return cachedData;

            string fullPath = _fileSystem.Path.Combine(_rootPath, path);
            if (!_fileSystem.File.Exists(fullPath))
            {
                throw new FileNotFoundException($"File not found: {path}");
            }

            byte[] data = await _fileSystem.File.ReadAllBytesAsync(fullPath, cancellationToken);
            await _cache.SetAsync(path, data, TimeSpan.FromMinutes(30), cancellationToken);

            return data;
        }

        public Task<bool> DeleteDataAsync(string path, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

            try
            {
                string fullPath = _fileSystem.Path.Combine(_rootPath, path);
                if (_fileSystem.File.Exists(fullPath))
                {
                    _fileSystem.File.Delete(fullPath);
                    _cache.RemoveAsync(path, cancellationToken).Wait(cancellationToken);
                }
                return Task.FromResult(true);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        public async Task<StorageMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
        {
            return new StorageMetrics
            {
                TotalSpace = await Task.Run(() => _fileSystem.DriveInfo.GetDrives()
                    .First(d => _rootPath.StartsWith(d.Name, StringComparison.OrdinalIgnoreCase))
                    .TotalSize, cancellationToken),
                AvailableSpace = await _monitor.GetAvailableSpaceAsync(cancellationToken)
            };
        }

        public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

            string fullPath = _fileSystem.Path.Combine(_rootPath, path);
            return Task.FromResult(_fileSystem.File.Exists(fullPath));
        }
    }
}