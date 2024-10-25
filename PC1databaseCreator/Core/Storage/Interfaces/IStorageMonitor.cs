using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using PC1databaseCreator.Core.Storage.Models;
using PC1databaseCreator.Core.Storage.Enums;
using PC1databaseCreator.Core.Storage.Common.Enums;

namespace PC1databaseCreator.Core.Storage.Interfaces
{
    public interface IStorageMonitor
    {
        // 실시간 모니터링
        Task<StorageResult<StorageMetrics>> GetCurrentMetricsAsync(StorageType storageType, CancellationToken cancellationToken = default);
        Task<StorageResult<StorageHealth>> CheckHealthAsync(StorageType storageType, CancellationToken cancellationToken = default);

        // 성능 메트릭
        Task<StorageResult<IReadOnlyList<StorageMetrics>>> GetHistoricalMetricsAsync(
            StorageType storageType,
            DateTime startTime,
            DateTime endTime,
            CancellationToken cancellationToken = default);

        Task<StorageResult<double>> GetAverageResponseTimeAsync(
            StorageType storageType,
            TimeSpan period,
            CancellationToken cancellationToken = default);

        // 알림 설정
        Task<StorageResult> SetAlertThresholdAsync(
            StorageType storageType,
            double threshold,
            StorageStatus status = StorageStatus.Warning,
            CancellationToken cancellationToken = default);

        // 이벤트 구독
        IDisposable SubscribeToHealthChanges(Action<StorageHealth> callback);
        IDisposable SubscribeToMetricsChanges(Action<StorageMetrics> callback);

        // 모니터링 제어
        Task<StorageResult> StartMonitoringAsync(MonitoringOptions options, CancellationToken cancellationToken = default);
        Task<StorageResult> StopMonitoringAsync(CancellationToken cancellationToken = default);
    }

    public record MonitoringOptions
    {
        public TimeSpan SamplingInterval { get; init; } = TimeSpan.FromSeconds(5);
        public bool EnableDetailedMetrics { get; init; } = false;
        public bool EnableHistoricalData { get; init; } = true;
        public TimeSpan DataRetentionPeriod { get; init; } = TimeSpan.FromDays(7);
    }
}