using System;
using System.Threading;
using System.Threading.Tasks;
using PC1databaseCreator.Core.Storage.Models;

namespace PC1databaseCreator.Core.Storage.Interfaces
{
    public interface IStorageService
    {
        // 기본 파일 작업
        Task<StorageResult<byte[]>> ReadDataAsync(string path, CancellationToken cancellationToken = default);
        Task<StorageResult> WriteDataAsync(string path, byte[] data, CancellationToken cancellationToken = default);
        Task<StorageResult> DeleteDataAsync(string path, CancellationToken cancellationToken = default);
        Task<StorageResult<bool>> ExistsAsync(string path, CancellationToken cancellationToken = default);

        // 드라이브 관리
        Task<StorageResult> InitializeDriveAsync(DriveConfig config, CancellationToken cancellationToken = default);
        Task<StorageResult<DriveConfig>> GetDriveConfigAsync(string path, CancellationToken cancellationToken = default);

        // 상태 확인
        Task<StorageResult<StorageMetrics>> GetMetricsAsync(StorageType storageType, CancellationToken cancellationToken = default);
        Task<StorageResult<StorageHealth>> CheckHealthAsync(StorageType storageType, CancellationToken cancellationToken = default);

        // 모니터링
        IDisposable RegisterChangeCallback(Action<StorageChange> callback);
    }
}