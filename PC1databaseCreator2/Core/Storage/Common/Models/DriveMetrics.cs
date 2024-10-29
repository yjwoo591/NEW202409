using PC1databaseCreator.Core.Storage.Common;

namespace PC1databaseCreator.Core.Storage.Models
{
    public class DriveMetrics
    {
        public long TotalSpace { get; private set; }
        public long UsedSpace { get; private set; }
        public long AvailableSpace { get; private set; }
        public double UsagePercentage => TotalSpace > 0 ? (UsedSpace * 100.0) / TotalSpace : 0;

        public long BytesRead { get; private set; }
        public long BytesWritten { get; private set; }
        public long ReadOperations { get; private set; }
        public long WriteOperations { get; private set; }

        public bool IsSpaceCritical => AvailableSpace < StorageConstants.Sizes.MinimumFreeSpace;
        public DateTime LastUpdated { get; private set; }

        public DriveMetrics()
        {
            LastUpdated = DateTime.UtcNow;
        }

        public void UpdateSpaceMetrics(long total, long used, long available)
        {
            TotalSpace = total;
            UsedSpace = used;
            AvailableSpace = available;
            LastUpdated = DateTime.UtcNow;
        }

        public void AddReadOperation(long bytes)
        {
            BytesRead += bytes;
            ReadOperations++;
            LastUpdated = DateTime.UtcNow;
        }

        public void AddWriteOperation(long bytes)
        {
            BytesWritten += bytes;
            WriteOperations++;
            LastUpdated = DateTime.UtcNow;
        }

        public void Reset()
        {
            BytesRead = 0;
            BytesWritten = 0;
            ReadOperations = 0;
            WriteOperations = 0;
            LastUpdated = DateTime.UtcNow;
        }

        public DriveMetricsSnapshot CreateSnapshot()
        {
            return new DriveMetricsSnapshot(
                TotalSpace,
                UsedSpace,
                AvailableSpace,
                BytesRead,
                BytesWritten,
                ReadOperations,
                WriteOperations,
                LastUpdated
            );
        }
    }

    public record DriveMetricsSnapshot
    {
        public long TotalSpace { get; init; }
        public long UsedSpace { get; init; }
        public long AvailableSpace { get; init; }
        public double UsagePercentage => TotalSpace > 0 ? (UsedSpace * 100.0) / TotalSpace : 0;
        public long BytesRead { get; init; }
        public long BytesWritten { get; init; }
        public long ReadOperations { get; init; }
        public long WriteOperations { get; init; }
        public bool IsSpaceCritical => AvailableSpace < StorageConstants.Sizes.MinimumFreeSpace;
        public DateTime LastUpdated { get; init; }

        public DriveMetricsSnapshot(
            long totalSpace,
            long usedSpace,
            long availableSpace,
            long bytesRead,
            long bytesWritten,
            long readOperations,
            long writeOperations,
            DateTime lastUpdated)
        {
            TotalSpace = totalSpace;
            UsedSpace = usedSpace;
            AvailableSpace = availableSpace;
            BytesRead = bytesRead;
            BytesWritten = bytesWritten;
            ReadOperations = readOperations;
            WriteOperations = writeOperations;
            LastUpdated = lastUpdated;
        }
    }
}