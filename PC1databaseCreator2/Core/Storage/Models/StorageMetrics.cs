
using System;
using System.Collections.Generic;
using System.Threading;

namespace PC1databaseCreator.Core.Storage.Models
{
    public class DriveMetrics
    {
        public string DrivePath { get; set; }
        public long TotalSpace { get; set; }
        public long UsedSpace { get; set; }
        public long FreeSpace { get; set; }
        public int IOPS { get; set; }
        public double Latency { get; set; }
        public double Throughput { get; set; }
        public DateTime LastUpdated { get; set; }
        public Dictionary<string, long> FileAccessCounts { get; private set; }
        public Queue<PerformanceRecord> PerformanceHistory { get; private set; }

        private long _readOperations;
        private long _writeOperations;
        private readonly object _lockObject = new object();
        private const int MAX_HISTORY_COUNT = 1000;

        public DriveMetrics()
        {
            FileAccessCounts = new Dictionary<string, long>();
            PerformanceHistory = new Queue<PerformanceRecord>();
            LastUpdated = DateTime.Now;
        }

        public void IncrementReadOperation()
        {
            Interlocked.Increment(ref _readOperations);
        }

        public void IncrementWriteOperation()
        {
            Interlocked.Increment(ref _writeOperations);
        }

        public void AddFileAccess(string filePath)
        {
            lock (_lockObject)
            {
                if (FileAccessCounts.ContainsKey(filePath))
                {
                    FileAccessCounts[filePath]++;
                }
                else
                {
                    FileAccessCounts[filePath] = 1;
                }
            }
        }

        public void UpdatePerformanceMetrics(double currentLatency, double currentThroughput)
        {
            lock (_lockObject)
            {
                PerformanceHistory.Enqueue(new PerformanceRecord
                {
                    Timestamp = DateTime.Now,
                    Latency = currentLatency,
                    Throughput = currentThroughput,
                    IOPS = IOPS
                });

                while (PerformanceHistory.Count > MAX_HISTORY_COUNT)
                {
                    PerformanceHistory.Dequeue();
                }

                Latency = currentLatency;
                Throughput = currentThroughput;
                LastUpdated = DateTime.Now;
            }
        }

        public void UpdateSpaceMetrics(long totalSpace, long usedSpace)
        {
            TotalSpace = totalSpace;
            UsedSpace = usedSpace;
            FreeSpace = totalSpace - usedSpace;
            LastUpdated = DateTime.Now;
        }

        public long GetTotalOperations()
        {
            return _readOperations + _writeOperations;
        }

        public double GetReadWriteRatio()
        {
            if (_writeOperations == 0)
                return 0;

            return (double)_readOperations / _writeOperations;
        }
    }

    public class PerformanceRecord
    {
        public DateTime Timestamp { get; set; }
        public double Latency { get; set; }
        public double Throughput { get; set; }
        public int IOPS { get; set; }
    }
}
