using System;
using System.Threading;
using System.Threading.Tasks;
using PC1databaseCreator.Core.Storage.Models;
using PC1databaseCreator.Core.Storage.Cache;

namespace PC1databaseCreator.Core.Storage.Base.Interfaces
{
    public interface IStorageCache
    {
        Task<StorageResult<byte[]>> GetAsync(string key, CancellationToken cancellationToken = default);
        Task<StorageResult> SetAsync(string key, byte[] value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
        Task<StorageResult> RemoveAsync(string key, CancellationToken cancellationToken = default);
        Task<StorageResult> ClearAsync(CancellationToken cancellationToken = default);
        Task<StorageResult<long>> GetCacheSizeAsync(CancellationToken cancellationToken = default);
        Task<StorageResult<int>> GetItemCountAsync(CancellationToken cancellationToken = default);
        Task<StorageResult> SetPolicyAsync(CachePolicy policy, CancellationToken cancellationToken = default);
    }
}