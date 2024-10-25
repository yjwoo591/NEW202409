using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using PC1databaseCreator.Core.Storage.Interfaces;
using PC1databaseCreator.Core.Storage.Models;
using PC1databaseCreator.Core.Storage.Constants;

namespace PC1databaseCreator.Core.Storage.Services
{
    public class StorageMetricsService
    {
        private readonly ILogger<StorageMetricsService> _logger;
        private readonly IStorageMonitor _monitor;
        private readonly ConcurrentDictionary<StorageType, List<MetricSnapshot>> _metricsHistory;
        private readonly ConcurrentDictionary<StorageType, PerformanceMetrics> _performanceMetrics;

        private readonly TimeSpan _historyRetention = TimeSpan.FromDays(7);
        private readonly int _maxHistoryItems = 10000;

        public StorageMetricsService(
            ILogger<StorageMetricsService> logger,
            IStorageMonitor monitor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            _metricsHistory = new ConcurrentDictionary<StorageType, List<MetricSnapshot>>();
            _performanceMetrics = new ConcurrentDictionary<StorageType, PerformanceMetrics>();

            InitializeMetrics();
        }

        private void InitializeMetrics()
        {
            foreach (StorageType storageType in Enum.GetValues(typeof(StorageType)))
            {
                _metricsHistory[storageType] = new List<MetricSnapshot>();
                _performanceMetrics[storageType] = new PerformanceMetrics();
            }
        }

        public async Task<StorageResult<StorageMetricsSummary>> GetMetricsSummaryAsync(
            StorageType storageType,
            TimeSpan period,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // 현재 메트릭 가져오기
                var currentMetrics = await _monitor.GetCurrentMetricsAsync(storageType, cancellationToken);
                if (!currentMetrics.IsSuccess)
                {
                    return StorageResult<StorageMetricsSummary>.Failure(
                        currentMetrics.ErrorType,
                        currentMetrics.Message,
                        currentMetrics.Exception);
                }

                // 성능 메트릭 가져오기
                var performance = _performanceMetrics.GetOrAdd(storageType, _ => new PerformanceMetrics());

                // 이력 데이터 분석
                var history = GetMetricsHistory(storageType, period);
                var trends = AnalyzeTrends(history);

                var summary = new StorageMetricsSummary
                {
                    StorageType = storageType,
                    CurrentMetrics = currentMetrics.Data,
                    AverageResponseTime = performance.GetAverageResponseTime(),
                    PeakUsage = history.Count > 0 ? history.Max(m => m.Metrics.UsagePercentage) : 0,
                    TrendAnalysis = trends,
                    SummaryTime = DateTime.UtcNow
                };

                return StorageResult<StorageMetricsSummary>.Success(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metrics summary for storage type: {StorageType}", storageType);
                return StorageResult<StorageMetricsSummary>.Failure(
                    StorageErrorType.InvalidOperation,
                    "Error getting metrics summary",
                    ex);
            }
        }

        public async Task<StorageResult> RecordMetricsAsync(
            StorageType storageType,
            StorageMetrics metrics,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var history = _metricsHistory.GetOrAdd(storageType, _ => new List<MetricSnapshot>());
                var performance = _performanceMetrics.GetOrAdd(storageType, _ => new PerformanceMetrics());

                lock (history)
                {
                    history.Add(new MetricSnapshot { Metrics = metrics, Timestamp = DateTime.UtcNow });

                    // 오래된 데이터 정리
                    var cutoff = DateTime.UtcNow - _historyRetention;
                    history.RemoveAll(m => m.Timestamp < cutoff);

                    // 최대 항목 수 제한
                    if (history.Count > _maxHistoryItems)
                    {
                        history.RemoveRange(0, history.Count - _maxHistoryItems);
                    }
                }

                // 성능 메트릭 업데이트
                performance.RecordMetrics(metrics);

                return StorageResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording metrics for storage type: {StorageType}", storageType);
                return StorageResult.Failure(
                    StorageErrorType.InvalidOperation,
                    "Error recording metrics",
                    ex);
            }
        }

        public async Task<StorageResult<PerformanceReport>> GetPerformanceReportAsync(
            StorageType storageType,
            TimeSpan period,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var history = GetMetricsHistory(storageType, period);
                var performance = _performanceMetrics.GetOrAdd(storageType, _ => new PerformanceMetrics());

                var report = new PerformanceReport
                {
                    StorageType = storageType,
                    Period = period,
                    AverageResponseTime = performance.GetAverageResponseTime(),
                    MaxResponseTime = performance.GetMaxResponseTime(),
                    TotalOperations = performance.GetTotalOperations(),
                    OperationsPerSecond = performance.GetOperationsPerSecond(),
                    AverageUsage = history.Count > 0 ? history.Average(m => m.Metrics.UsagePercentage) : 0,
                    PeakUsage = history.Count > 0 ? history.Max(m => m.Metrics.UsagePercentage) : 0,
                    GeneratedTime = DateTime.UtcNow
                };

                return StorageResult<PerformanceReport>.Success(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance report for storage type: {StorageType}", storageType);
                return StorageResult<PerformanceReport>.Failure(
                    StorageErrorType.InvalidOperation,
                    "Error getting performance report",
                    ex);
            }
        }

        private List<MetricSnapshot> GetMetricsHistory(StorageType storageType, TimeSpan period)
        {
            var history = _metricsHistory.GetOrAdd(storageType, _ => new List<MetricSnapshot>());
            var cutoff = DateTime.UtcNow - period;

            lock (history)
            {
                return history.Where(m => m.Timestamp >= cutoff).ToList();
            }
        }

        private TrendAnalysis AnalyzeTrends(List<MetricSnapshot> history)
        {
            if (history.Count < 2)
            {
                return new TrendAnalysis
                {
                    UsageTrend = 0,
                    ResponseTimeTrend = 0,
                    Prediction = "Insufficient data for trend analysis"
                };
            }

            var oldestUsage = history.First().Metrics.UsagePercentage;
            var newestUsage = history.Last().Metrics.UsagePercentage;
            var usageTrend = (newestUsage - oldestUsage) / history.Count;

            var oldestResponse = history.First().Metrics.AverageResponseTimeMs;
            var newestResponse = history.Last().Metrics.AverageResponseTimeMs;
            var responseTrend = (newestResponse - oldestResponse) / history.Count;

            var prediction = "Stable";
            if (usageTrend > 1.0)
            {
                prediction = "Usage is increasing significantly";
            }
            else if (responseTrend > 0.1)
            {
                prediction = "Response times are degrading";
            }

            return new TrendAnalysis
            {
                UsageTrend = usageTrend,
                ResponseTimeTrend = responseTrend,
                Prediction = prediction
            };
        }

        private class MetricSnapshot
        {
            public StorageMetrics Metrics { get; init; }
            public DateTime Timestamp { get; init; }
        }

        private class PerformanceMetrics
        {
            private readonly ConcurrentQueue<double> _responseTimes = new();
            private readonly ConcurrentQueue<DateTime> _operationTimes = new();
            private long _totalOperations;
            private double _maxResponseTime;

            public void RecordMetrics(StorageMetrics metrics)
            {
                _responseTimes.Enqueue(metrics.AverageResponseTimeMs);
                _operationTimes.Enqueue(DateTime.UtcNow);
                _maxResponseTime = Math.Max(_maxResponseTime, metrics.AverageResponseTimeMs);
                Interlocked.Increment(ref _totalOperations);

                // 1시간 이상 된 데이터 정리
                var cutoff = DateTime.UtcNow - TimeSpan.FromHours(1);
                while (_operationTimes.TryPeek(out var oldest) && oldest < cutoff)
                {
                    _operationTimes.TryDequeue(out _);
                    _responseTimes.TryDequeue(out _);
                }
            }

            public double GetAverageResponseTime()
            {
                return _responseTimes.Count > 0 ? _responseTimes.Average() : 0;
            }

            public double GetMaxResponseTime()
            {
                return _maxResponseTime;
            }

            public long GetTotalOperations()
            {
                return _totalOperations;
            }

            public double GetOperationsPerSecond()
            {
                var recentOperations = _operationTimes.Count(t => t >= DateTime.UtcNow - TimeSpan.FromSeconds(1));
                return recentOperations;
            }
        }
    }

    public record StorageMetricsSummary
    {
        public StorageType StorageType { get; init; }
        public StorageMetrics CurrentMetrics { get; init; }
        public double AverageResponseTime { get; init; }
        public double PeakUsage { get; init; }
        public TrendAnalysis TrendAnalysis { get; init; }
        public DateTime SummaryTime { get; init; }
    }

    public record TrendAnalysis
    {
        public double UsageTrend { get; init; }
        public double ResponseTimeTrend { get; init; }
        public string Prediction { get; init; }
    }

    public record PerformanceReport
    {
        public StorageType StorageType { get; init; }
        public TimeSpan Period { get; init; }
        public double AverageResponseTime { get; init; }
        public double MaxResponseTime { get; init; }
        public long TotalOperations { get; init; }
        public double OperationsPerSecond { get; init; }
        public double AverageUsage { get; init; }
        public double PeakUsage { get; init; }
        public DateTime GeneratedTime { get; init; }
    }
}