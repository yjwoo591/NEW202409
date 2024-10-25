using System;
using System.IO.Compression;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PC1databaseCreator.Core.Storage.Models;
using PC1databaseCreator.Core.Storage.Constants;
using PC1databaseCreator.Core.Storage.Interfaces;

namespace PC1databaseCreator.Core.Storage.Cache
{
    public class StorageCache : IStorageCache, IDisposable
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<StorageCache> _logger;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks;
        private readonly CachePolicy _defaultPolicy;
        private CachePolicy _currentPolicy;
        private bool _disposed;

        public StorageCache(
            IMemoryCache cache,
            ILogger<StorageCache> logger,
            CachePolicy defaultPolicy = null)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _locks = new ConcurrentDictionary<string, SemaphoreSlim>();
            _defaultPolicy = defaultPolicy ?? new CachePolicy();
            _currentPolicy = _defaultPolicy;
        }

        public async Task<StorageResult<byte[]>> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(key))
                return StorageResult<byte[]>.Failure(StorageErrorType.InvalidOperation, "Cache key cannot be null or empty");

            try
            {
                var lockObj = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
                await lockObj.WaitAsync(cancellationToken);

                try
                {
                    if (_cache.TryGetValue(key, out CacheItem cacheItem))
                    {
                        byte[] data = cacheItem.Data;
                        if (_currentPolicy.EnableCompression && cacheItem.IsCompressed)
                        {
                            data = await DecompressDataAsync(data, cancellationToken);
                        }

                        _logger.LogDebug("Cache hit for key: {Key}", key);
                        return StorageResult<byte[]>.Success(data);
                    }

                    _logger.LogDebug("Cache miss for key: {Key}", key);
                    return StorageResult<byte[]>.Success(null);
                }
                finally
                {
                    lockObj.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving from cache for key: {Key}", key);
                return StorageResult<byte[]>.Failure(StorageErrorType.InvalidOperation, "Error retrieving from cache", ex);
            }
        }

        public async Task<StorageResult> SetAsync(string key, byte[] value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(key))
                return StorageResult.Failure(StorageErrorType.InvalidOperation, "Cache key cannot be null or empty");
            if (value == null)
                return StorageResult.Failure(StorageErrorType.InvalidOperation, "Cache value cannot be null");

            try
            {
                var lockObj = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
                await lockObj.WaitAsync(cancellationToken);

                try
                {
                    var currentSize = await GetCacheSizeAsync(cancellationToken);
                    if (!currentSize.IsSuccess)
                    {
                        return StorageResult.Failure(currentSize.ErrorType, currentSize.Message);
                    }

                    if (currentSize.Data + value.Length > _currentPolicy.MaxCacheSize)
                    {
                        _logger.LogWarning("Cache size limit exceeded. Clearing cache.");
                        await ClearAsync(cancellationToken);
                    }

                    byte[] dataToStore = value;
                    bool isCompressed = false;

                    if (_currentPolicy.EnableCompression && value.Length > StorageConstants.LARGE_FILE_THRESHOLD)
                    {
                        dataToStore = await CompressDataAsync(value, cancellationToken);
                        isCompressed = true;
                    }

                    var cacheItem = new CacheItem
                    {
                        Data = dataToStore,
                        IsCompressed = isCompressed,
                        Size = value.Length,
                        LastAccess = DateTime.UtcNow
                    };

                    var cacheEntryOptions = new MemoryCacheEntryOptions
                    {
                        Size = cacheItem.Size,
                        SlidingExpiration = expiration ?? _currentPolicy.DefaultExpiration
                    };

                    _cache.Set(key, cacheItem, cacheEntryOptions);
                    _logger.LogDebug("Successfully cached data for key: {Key}", key);

                    return StorageResult.Success();
                }
                finally
                {
                    lockObj.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for key: {Key}", key);
                return StorageResult.Failure(StorageErrorType.InvalidOperation, "Error setting cache", ex);
            }
        }

        public async Task<StorageResult> RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(key))
                return StorageResult.Failure(StorageErrorType.InvalidOperation, "Cache key cannot be null or empty");

            try
            {
                var lockObj = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
                await lockObj.WaitAsync(cancellationToken);

                try
                {
                    _cache.Remove(key);
                    _logger.LogDebug("Successfully removed cache entry for key: {Key}", key);
                    return StorageResult.Success();
                }
                finally
                {
                    lockObj.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache entry for key: {Key}", key);
                return StorageResult.Failure(StorageErrorType.InvalidOperation, "Error removing cache entry", ex);
            }
        }

        public async Task<StorageResult> ClearAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_cache is MemoryCache memoryCache)
                {
                    memoryCache.Compact(1.0);
                }
                _logger.LogInformation("Cache cleared successfully");
                return StorageResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache");
                return StorageResult.Failure(StorageErrorType.InvalidOperation, "Error clearing cache", ex);
            }
        }

        public async Task<StorageResult<long>> GetCacheSizeAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_cache is MemoryCache memoryCache)
                {
                    var size = memoryCache.Count * StorageConstants.DEFAULT_BUFFER_SIZE;
                    return StorageResult<long>.Success(size);
                }
                return StorageResult<long>.Success(0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache size");
                return StorageResult<long>.Failure(StorageErrorType.InvalidOperation, "Error getting cache size", ex);
            }
        }

        public async Task<StorageResult<int>> GetItemCountAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_cache is MemoryCache memoryCache)
                {
                    return StorageResult<int>.Success(memoryCache.Count);
                }
                return StorageResult<int>.Success(0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting item count");
                return StorageResult<int>.Failure(StorageErrorType.InvalidOperation, "Error getting item count", ex);
            }
        }

        public async Task<StorageResult> SetPolicyAsync(CachePolicy policy, CancellationToken cancellationToken = default)
        {
            try
            {
                _currentPolicy = policy ?? _defaultPolicy;
                _logger.LogInformation("Cache policy updated successfully");
                return StorageResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache policy");
                return StorageResult.Failure(StorageErrorType.InvalidOperation, "Error setting cache policy", ex);
            }
        }

        private async Task<byte[]> CompressDataAsync(byte[] data, CancellationToken cancellationToken)
        {
            using var memoryStream = new MemoryStream();
            using (var gzipStream = new GZipStream(memoryStream, CompressionLevel.Optimal))
            {
                await gzipStream.WriteAsync(data, 0, data.Length, cancellationToken);
            }
            return memoryStream.ToArray();
        }

        private async Task<byte[]> DecompressDataAsync(byte[] compressedData, CancellationToken cancellationToken)
        {
            using var compressedStream = new MemoryStream(compressedData);
            using var decompressStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();

            await decompressStream.CopyToAsync(resultStream, 81920, cancellationToken);
            return resultStream.ToArray();
        }

        public void Dispose()
        {
            if (_disposed) return;

            foreach (var lockObj in _locks.Values)
            {
                lockObj.Dispose();
            }
            _locks.Clear();

            _disposed = true;
        }

        private class CacheItem
        {
            public byte[] Data { get; set; }
            public bool IsCompressed { get; set; }
            public long Size { get; set; }
            public DateTime LastAccess { get; set; }
        }
    }

    public record CachePolicy
    {
        public TimeSpan DefaultExpiration { get; init; } = TimeSpan.FromMinutes(30);
        public long MaxCacheSize { get; init; } = StorageConstants.Cache.MAX_CACHE_SIZE;
        public int MaxItems { get; init; } = StorageConstants.Cache.MAX_CACHE_ITEMS;
        public bool EnableCompression { get; init; } = true;
    }
}