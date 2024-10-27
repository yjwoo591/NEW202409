```csharp
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PC1databaseCreator.Core.Storage.Base.Interfaces;

namespace PC1databaseCreator.Core.Storage.Monitoring
{
    /// <summary>
    /// 스토리지 작업의 성능 지표를 수집하고 관리하는 클래스
    /// </summary>
    public class StorageMetricsCollector : IStorageMetrics
    {
        private readonly ILogger<StorageMetricsCollector> _logger;
        private readonly ConcurrentDictionary<Guid, OperationMetrics> _operationMetrics;
        private readonly ConcurrentDictionary<StorageOperationType, OperationTypeMetrics> _typeMetrics;

        public StorageMetricsCollector(ILogger<StorageMetricsCollector> logger)
        {
            _logger = logger;
            _operationMetrics = new ConcurrentDictionary<Guid, OperationMetrics>();
            _typeMetrics = new ConcurrentDictionary<StorageOperationType, OperationTypeMetrics>();
        }

        /// <summary>
        /// 작업 시작 기록
        /// </summary>
        public void RecordStart(Guid operationId, StorageOperationType type)
        {
            var metrics = new OperationMetrics
            {
                OperationType = type,
                StartTime = DateTime.UtcNow
            };

            _operationMetrics.TryAdd(operationId, metrics);
            _typeMetrics.AddOrUpdate(
                type,
                new OperationTypeMetrics { OperationCount = 1 },
                (_, existing) =>
                {
                    existing.OperationCount++;
                    return existing;
                });

            _logger.LogTrace("Operation started - ID: {OperationId}, Type: {OperationType}",
                operationId, type);
        }

        /// <summary>
        /// 작업 완료 기록
        /// </summary>
        public void RecordComplete(IStorageResult result)
        {
            if (_operationMetrics.TryGetValue(result.OperationId, out var metrics))
            {
                metrics.EndTime = DateTime.UtcNow;
                metrics.IsSuccess = result.IsSuccess;
                metrics.Duration = metrics.EndTime - metrics.StartTime;

                UpdateTypeMetrics(metrics);

                _logger.LogTrace(
                    "Operation completed - ID: {OperationId}, Success: {Success}, Duration: {Duration}ms",
                    result.OperationId, result.IsSuccess, metrics.Duration.TotalMilliseconds);
            }
        }

        /// <summary>
        /// 오류 기록
        /// </summary>
        public void RecordError(Guid operationId, Exception error)
        {
            if (_operationMetrics.TryGetValue(operationId, out var metrics))
            {
                metrics.EndTime = DateTime.UtcNow;
                metrics.IsSuccess = false;
                metrics.Error = error;
                metrics.Duration = metrics.EndTime - metrics.StartTime;

                UpdateTypeMetrics(metrics);

                _logger.LogError(error,
                    "Operation failed - ID: {OperationId}, Type: {OperationType}, Duration: {Duration}ms",
                    operationId, metrics.OperationType, metrics.Duration.TotalMilliseconds);
            }
        }

        /// <summary>
        /// 메모리 사용량 기록
        /// </summary>
        public void RecordMemoryUsage(Guid operationId, long bytes)
        {
            if (_operationMetrics.TryGetValue(operationId, out var metrics))
            {
                metrics.MemoryUsed = bytes;

                _typeMetrics.AddOrUpdate(
                    metrics.OperationType,
                    new OperationTypeMetrics { TotalMemoryUsed = bytes },
                    (_, existing) =>
                    {
                        existing.TotalMemoryUsed += bytes;
                        return existing;
                    });

                _logger.LogTrace(
                    "Memory usage recorded - ID: {OperationId}, Bytes: {Bytes}",
                    operationId, bytes);
            }
        }

        /// <summary>
        /// 성능 보고서 생성
        /// </summary>
        public async Task<StorageMetricsReport> GenerateReportAsync(
            DateTime startTime,
            DateTime endTime)
        {
            var report = new StorageMetricsReport
            {
                StartTime = startTime,
                EndTime = endTime
            };

            foreach (var metrics in _operationMetrics.Values)
            {
                if (metrics.StartTime >= startTime && metrics.EndTime <= endTime)
                {
                    report.TotalOperations++;
                    if (metrics.IsSuccess)
                        report.SuccessfulOperations++;
                    else
                        report.FailedOperations++;

                    report.OperationTypeCounts.TryAdd(metrics.OperationType, 0);
                    report.OperationTypeCounts[metrics.OperationType]++;
                }
            }

            // 평균 계산
            if (report.TotalOperations > 0)
            {
                var totalDuration = 0.0;
                var totalMemory = 0.0;

                foreach (var metrics in _operationMetrics.Values)
                {
                    if (metrics.StartTime >= startTime && metrics.EndTime <= endTime)
                    {
                        totalDuration += metrics.Duration.TotalMilliseconds;
                        totalMemory += metrics.MemoryUsed;
                    }
                }

                report.AverageOperationTime = totalDuration / report.TotalOperations;
                report.AverageMemoryUsage = totalMemory / report.TotalOperations;
            }

            return report;
        }

        private void UpdateTypeMetrics(OperationMetrics metrics)
        {
            _typeMetrics.AddOrUpdate(
                metrics.OperationType,
                new OperationTypeMetrics
                {
                    OperationCount = 1,
                    SuccessCount = metrics.IsSuccess ? 1 : 0,
                    FailureCount = metrics.IsSuccess ? 0 : 1,
                    TotalDuration = metrics.Duration.TotalMilliseconds
                },
                (_, existing) =>
                {
                    if (metrics.IsSuccess)
                        existing.SuccessCount++;
                    else
                        existing.FailureCount++;

                    existing.TotalDuration += metrics.Duration.TotalMilliseconds;
                    return existing;
                });
        }

        private class OperationMetrics
        {
            public StorageOperationType OperationType { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public TimeSpan Duration { get; set; }
            public bool IsSuccess { get; set; }
            public Exception Error { get; set; }
            public long MemoryUsed { get; set; }
        }

        private class OperationTypeMetrics
        {
            public long OperationCount { get; set; }
            public long SuccessCount { get; set; }
            public long FailureCount { get; set; }
            public double TotalDuration { get; set; }
            public long TotalMemoryUsed { get; set; }
        }
    }
}
```