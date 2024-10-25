using System;
using PC1databaseCreator.Core.Storage.Common.Enums;
using PC1databaseCreator.Core.Storage.Enums;

namespace PC1databaseCreator.Core.Storage.Models
{
    public record StorageMetrics
    {
        public StorageType StorageType { get; init; }
        public long TotalSpace { get; init; }
        public long AvailableSpace { get; init; }
        public long UsedSpace { get; init; }
        public double UsagePercentage => TotalSpace > 0 ? (double)UsedSpace / TotalSpace * 100 : 0;
        public DateTime MeasurementTime { get; init; } = DateTime.UtcNow;
        public int IoOperationsPerSecond { get; init; }
        public double AverageResponseTimeMs { get; init; }
        public StorageStatus Status { get; init; }

        public bool IsHealthy => Status == StorageStatus.OK;
    }
}
