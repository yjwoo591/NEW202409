using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PC1databaseCreator.Core.Storage.Interfaces;
using PC1databaseCreator.Core.Storage.Models;
using PC1databaseCreator.Core.Storage.Enums;
using PC1databaseCreator.Core.Storage.Common.Enums;

namespace PC1databaseCreator.Core.Storage.Infrastructure
{
    public class StorageMonitor : IStorageMonitor
    {
        private readonly ILogger<StorageMonitor> _logger;
        private readonly ConcurrentDictionary<StorageType, FileSystemProvider> _providers;
        private readonly ConcurrentDictionary<StorageType, List<StorageMetrics>> _metricsHistory;
        private readonly ConcurrentDictionary<Guid, Action<StorageHealth>> _healthSubscribers;
        private readonly ConcurrentDictionary<Guid, Action<StorageMetrics>> _metricsSubscribers;

        private CancellationTokenSource _monitoringTokenSource;
        private Task _monitoringTask;
        private MonitoringOptions _currentOptions;

        public StorageMonitor(
            ILogger<StorageMonitor> logger,
            IDictionary<StorageType, FileSystemProvider> providers)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _providers = new ConcurrentDictionary<StorageType, FileSystemProvider>(
                providers ?? throw new ArgumentNullException(nameof(providers)));

            _metricsHistory = new ConcurrentDictionary<StorageType, List<StorageMetrics>>();
            _healthSubscribers = new ConcurrentDictionary<Guid, Action<StorageHealth>>();
            _metricsSubscribers = new ConcurrentDictionary<Guid, Action<StorageMetrics>>();
        }

        public async Task<StorageResult<StorageMetrics>> GetCurrentMetricsAsync(
            StorageType storageType,
            CancellationToken cancellationToken = default)
        {
            if (!_providers.TryGetValue(storageType, out var provider))
            {
                return StorageResult<StorageMetrics>.Failure(
                    StorageErrorType.InvalidOperation,
                    $"Provider not found for storage type: {storageType}");
            }

            return provider.GetMetrics();
        }

        public async Task<StorageResult<StorageHealth>> CheckHealthAsync(
            StorageType storageType,
            CancellationToken cancellationToken = default)
        {
            var metricsResult = await GetCurrentMetricsAsync(storageType, cancellationToken);
            if (!metricsResult.IsSuccess)
            {
                return StorageResult<StorageHealth>.Failure(
                    metricsResult.ErrorType,
                    metricsResult.Message,
                    metricsResult.Exception);
            }

            var health = new StorageHealth
            {
                StorageType = storageType,
                Status = metricsResult.Data.Status,
                Metrics = metricsResult.Data,
                Messages = new List<string>()
            };

            if (health.Status != StorageStatus.OK)
            {
                health.Messages.Add($"Storage usage at {metricsResult.Data.UsagePercentage:F1}%");
            }

            NotifyHealthSubscribers(health);
            return StorageResult<StorageHealth>.Success(health);
        }

        public async Task<StorageResult<IReadOnlyList<StorageMetrics>>> GetHistoricalMetricsAsync(
            StorageType storageType,
            DateTime startTime,
            DateTime endTime,
            CancellationToken cancellationToken = default)
        {
            if (!_metricsHistory.TryGetValue(storageType, out var history))
            {
                return StorageResult<IReadOnlyList<StorageMetrics>>.Success(
                    new List<StorageMetrics>());
            }

            var metrics = history
                .Where(m => m.MeasurementTime >= startTime && m.MeasurementTime <= endTime)
                .ToList();

            return StorageResult<IReadOnlyList<StorageMetrics>>.Success(metrics);
        }

        public async Task<StorageResult<double>> GetAverageResponseTimeAsync(
            StorageType storageType,
            TimeSpan period,
            CancellationToken cancellationToken = default)
        {
            if (!_metricsHistory.TryGetValue(storageType, out var history))
            {
                return StorageResult<double>.Success(0);
            }

            var cutoff = DateTime.UtcNow - period;
            var average = history
                .Where(m => m.MeasurementTime >= cutoff)
                .Average(m => m.AverageResponseTimeMs);

            return StorageResult<double>.Success(average);
        }

        public IDisposable SubscribeToHealthChanges(Action<StorageHealth> callback)
        {
            var id = Guid.NewGuid();
            _healthSubscribers[id] = callback;
            return new SubscriptionToken(() => _healthSubscribers.TryRemove(id, out _));
        }

        public IDisposable SubscribeToMetricsChanges(Action<StorageMetrics> callback)
        {
            var id = Guid.NewGuid();
            _metricsSubscribers[id] = callback;
            return new SubscriptionToken(() => _metricsSubscribers.TryRemove(id, out _));
        }

        private void NotifyHealthSubscribers(StorageHealth health)
        {
            foreach (var subscriber in _healthSubscribers.Values)
            {
                try
                {
                    subscriber(health);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error notifying health subscriber");
                }
            }
        }

        private void NotifyMetricsSubscribers(StorageMetrics metrics)
        {
            foreach (var subscriber in _metricsSubscribers.Values)
            {
                try
                {
                    subscriber(metrics);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error notifying metrics subscriber");
                }
            }
        }

        public async Task<StorageResult> StartMonitoringAsync(
            MonitoringOptions options,
            CancellationToken cancellationToken = default)
        {
            if (_monitoringTask != null)
            {
                return StorageResult.Failure(
                    StorageErrorType.InvalidOperation,
                    "Monitoring is already running");
            }

            _currentOptions = options;
            _monitoringTokenSource = new CancellationTokenSource();

            _monitoringTask = MonitoringLoopAsync(_monitoringTokenSource.Token);

            return StorageResult.Success();
        }

        public async Task<StorageResult> StopMonitoringAsync(CancellationToken cancellationToken = default)
        {
            if (_monitoringTask == null)
            {
                return StorageResult.Success("Monitoring is not running");
            }

            try
            {
                _monitoringTokenSource.Cancel();
                await _monitoringTask;
                return StorageResult.Success();
            }
            catch (Exception ex)
            {
                return StorageResult.Failure(
                    StorageErrorType.InvalidOperation,
                    "Error stopping monitoring",
                    ex);
            }
            finally
            {
                _monitoringTask = null;
                _monitoringTokenSource?.Dispose();
                _monitoringTokenSource = null;
            }
        }

        private async Task MonitoringLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    foreach (var type in _providers.Keys)
                    {
                        var metricsResult = await GetCurrentMetricsAsync(type, cancellationToken);
                        if (metricsResult.IsSuccess)
                        {
                            if (_currentOptions.EnableHistoricalData)
                            {
                                StoreMetrics(type, metricsResult.Data);
                            }
                            NotifyMetricsSubscribers(metricsResult.Data);

                            await CheckHealthAsync(type, cancellationToken);
                        }
                    }

                    await Task.Delay(_currentOptions.SamplingInterval, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in monitoring loop");
                }
            }
        }

        private void StoreMetrics(StorageType type, StorageMetrics metrics)
        {
            var history = _metricsHistory.GetOrAdd(type, _ => new List<StorageMetrics>());

            lock (history)
            {
                history.Add(metrics);

                // 오래된 데이터 정리
                var cutoff = DateTime.UtcNow - _currentOptions.DataRetentionPeriod;
                history.RemoveAll(m => m.MeasurementTime < cutoff);
            }
        }

        private class SubscriptionToken : IDisposable
        {
            private readonly Action _unsubscribeAction;
            private bool _disposed;

            public SubscriptionToken(Action unsubscribeAction)
            {
                _unsubscribeAction = unsubscribeAction;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _unsubscribeAction?.Invoke();
                _disposed = true;
            }
        }
    }
}