using PC1databaseCreator.Core.Storage.Enums;
using ErrorOr;

namespace PC1databaseCreator.Core.Storage.Infrastructure.Interfaces
{
    public interface IStorageMonitor
    {
        StorageStatus GetStorageStatus(StorageType storageType);
        void UpdateStorageStatus(StorageType storageType, StorageStatus newStatus);
        void LogStorageOperation(StorageType storageType, StorageOperation operation, string details);
        bool IsStorageReady(StorageType storageType);
        Task<ErrorOr<bool>> SetAlertThresholdAsync(
            StorageType storageType,
            double threshold,
            StorageStatus alertStatus,
            CancellationToken cancellationToken = default);
    }
}