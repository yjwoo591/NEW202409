using Microsoft.Extensions.Logging;
using PC1databaseCreator.Core.Storage.Enums;
using PC1databaseCreator.Core.Storage.Infrastructure.Interfaces;
using ErrorOr;

namespace PC1databaseCreator.Core.Storage.Infrastructure
{
    public class StorageMonitor : IStorageMonitor
    {
        private readonly ILogger<StorageMonitor> _logger;
        private readonly Dictionary<StorageType, StorageStatus> _storageStatus;
        private readonly Dictionary<StorageType, (double Threshold, StorageStatus AlertStatus)> _alertThresholds;

        public StorageMonitor(ILogger<StorageMonitor> logger)
        {
            _logger = logger;
            _storageStatus = new Dictionary<StorageType, StorageStatus>
            {
                { StorageType.SSD1, StorageStatus.Ready },
                { StorageType.SSD2, StorageStatus.Ready },
                { StorageType.HDD, StorageStatus.Ready }
            };
            _alertThresholds = new Dictionary<StorageType, (double, StorageStatus)>();
        }

        public StorageStatus GetStorageStatus(StorageType storageType)
        {
            return _storageStatus.TryGetValue(storageType, out var status)
                ? status
                : StorageStatus.Offline;
        }

        public void UpdateStorageStatus(StorageType storageType, StorageStatus newStatus)
        {
            _storageStatus[storageType] = newStatus;
            _logger.LogInformation("Storage {Type} status updated to {Status}",
                storageType, newStatus);

            // 임계값 확인 및 알림 처리
            CheckThresholdAndNotify(storageType);
        }

        public void LogStorageOperation(StorageType storageType, StorageOperation operation, string details)
        {
            _logger.LogInformation(
                "Storage operation - Type: {StorageType}, Operation: {Operation}, Details: {Details}",
                storageType, operation, details);
        }

        public bool IsStorageReady(StorageType storageType)
        {
            return GetStorageStatus(storageType) == StorageStatus.Ready;
        }

        public async Task<ErrorOr<bool>> SetAlertThresholdAsync(
            StorageType storageType,
            double threshold,
            StorageStatus alertStatus,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (threshold < 0 || threshold > 100)
                    return Error.Validation("Threshold must be between 0 and 100");

                await Task.Run(() =>
                {
                    _alertThresholds[storageType] = (threshold, alertStatus);
                    _logger.LogInformation(
                        "Alert threshold set for {StorageType}: {Threshold}%, Status: {AlertStatus}",
                        storageType, threshold, alertStatus);
                }, cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set alert threshold for {StorageType}", storageType);
                return Error.Failure(ex.Message);
            }
        }

        private void CheckThresholdAndNotify(StorageType storageType)
        {
            if (_alertThresholds.TryGetValue(storageType, out var threshold))
            {
                // 여기서 실제 스토리지 사용량을 확인하고 임계값과 비교하는 로직 구현
                var currentUsage = GetStorageUsage(storageType);
                if (currentUsage >= threshold.Threshold)
                {
                    _logger.LogWarning(
                        "Storage {Type} usage ({Usage}%) exceeded threshold ({Threshold}%)",
                        storageType, currentUsage, threshold.Threshold);

                    UpdateStorageStatus(storageType, threshold.AlertStatus);
                }
            }
        }

        private double GetStorageUsage(StorageType storageType)
        {
            // 실제 스토리지 사용량을 확인하는 로직 구현
            // 임시로 랜덤 값 반환
            return new Random().NextDouble() * 100;
        }
    }
}