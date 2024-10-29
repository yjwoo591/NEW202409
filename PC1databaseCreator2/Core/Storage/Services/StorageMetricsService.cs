using PC1databaseCreator.Common.Infrastructure.Interfaces.Logging;
using PC1databaseCreator.Core.Storage.Models;
using System.Collections.Concurrent;

namespace PC1databaseCreator.Core.Storage.Services
{
    public class StorageMetricsService
    {
        private readonly ILoggerService _logger;
        private readonly ConcurrentDictionary<string, DriveMetrics> _driveMetrics;

        public StorageMetricsService(ILoggerService logger)
        {
            _logger = logger;
            _driveMetrics = new ConcurrentDictionary<string, DriveMetrics>();
        }

        public void RecordOperation(string drivePath, long bytesProcessed, bool isWrite)
        {
            var metrics = _driveMetrics.GetOrAdd(drivePath, _ => new DriveMetrics());

            if (isWrite)
            {
                Interlocked.Add(ref metrics._bytesWritten, bytesProcessed);
                Interlocked.Increment(ref metrics._writeOperations);
            }
            else
            {
                Interlocked.Add(ref metrics._bytesRead, bytesProcessed);
                Interlocked.Increment(ref metrics._readOperations);
            }
        }

        public DriveMetricsSnapshot GetMetrics(string drivePath)
        {
            if (_driveMetrics.TryGetValue(drivePath, out var metrics))
            {
                return new DriveMetricsSnapshot
                {
                    BytesRead = Interlocked.Read(ref metrics._bytesRead),
                    BytesWritten = Interlocked.Read(ref metrics._bytesWritten),
                    ReadOperations = Interlocked.Read(ref metrics._readOperations),
                    WriteOperations = Interlocked.Read(ref metrics._writeOperations),
                    LastUpdated = metrics.LastUpdated
                };
            }

            return new DriveMetricsSnapshot();
        }

        public void ResetMetrics(string drivePath)
        {
            if (_driveMetrics.TryGetValue(drivePath, out var metrics))
            {
                metrics.Reset();
            }
        }
    }

    internal class DriveMetrics
    {
        internal long _bytesRead;
        internal long _bytesWritten;
        internal long _readOperations;
        internal long _writeOperations;
        internal DateTime LastUpdated;

        public void Reset()
        {
            Interlocked.Exchange(ref _bytesRead, 0);
            Interlocked.Exchange(ref _bytesWritten, 0);
            Interlocked.Exchange(ref _readOperations, 0);
            Interlocked.Exchange(ref _writeOperations, 0);
            LastUpdated = DateTime.UtcNow;
        }
    }

    public class DriveMetricsSnapshot
    {
        public long BytesRead { get; init; }
        public long BytesWritten { get; init; }
        public long ReadOperations { get; init; }
        public long WriteOperations { get; init; }
        public DateTime LastUpdated { get; init; }
    }
}