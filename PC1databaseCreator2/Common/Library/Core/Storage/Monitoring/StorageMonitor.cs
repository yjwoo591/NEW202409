using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Timer = System.Threading.Timer;
using PC1databaseCreator.Common.Library.Core.Storage.Models;

namespace PC1databaseCreator.Common.Library.Core.Storage.Monitoring
{
    /// <summary>
    /// 스토리지 모니터링 기본 클래스
    /// </summary>
    public class StorageMonitor : IDisposable
    {
        private readonly ILogger _logger;
        private readonly Timer _monitoringTimer;
        private readonly CancellationTokenSource _cancellationTokenSource;
        protected readonly StorageMetrics Metrics;

        public event EventHandler<StorageMetrics> MetricsUpdated;

        public StorageMonitor(ILogger logger, TimeSpan interval)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Metrics = new StorageMetrics();
            _cancellationTokenSource = new CancellationTokenSource();
            _monitoringTimer = new Timer(MonitoringCallback, null, interval, interval);
        }

        private async void MonitoringCallback(object state)
        {
            try
            {
                await UpdateMetricsAsync();
                MetricsUpdated?.Invoke(this, Metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during storage monitoring");
            }
        }

        protected virtual Task UpdateMetricsAsync()
        {
            // 기본 구현은 비어있음 - 파생 클래스에서 구현
            return Task.CompletedTask;
        }

        public virtual void Dispose()
        {
            _monitoringTimer?.Dispose();
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}