using Serilog;
using Serilog.Events;
using Serilog.Core;
using Serilog.Configuration;
using PC1databaseCreator.Common.Infrastructure.Interfaces.Logging;

namespace PC1databaseCreator.Common.Library.Logging
{
    public sealed class LoggerService : ILoggerService
    {
        private readonly ILogger _logger;

        public LoggerService(LoggerConfiguration configuration)
        {
            _logger = new Serilog.LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", configuration.MinimumLevel)
                .MinimumLevel.Override("System", configuration.MinimumLevel)
                .Enrich.FromLogContext()
                .WriteTo.File(
                    path: configuration.LogFilePath,
                    restrictedToMinimumLevel: configuration.MinimumLevel,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: configuration.RetentionDays)
                .CreateLogger();
        }

        public void LogTrace(string message, params object[] args)
            => _logger.Verbose(message, args);

        public void LogDebug(string message, params object[] args)
            => _logger.Debug(message, args);

        public void LogInformation(string message, params object[] args)
            => _logger.Information(message, args);

        public void LogWarning(string message, params object[] args)
            => _logger.Warning(message, args);

        public void LogError(Exception exception, string message, params object[] args)
            => _logger.Error(exception, message, args);

        public void LogCritical(Exception exception, string message, params object[] args)
            => _logger.Fatal(exception, message, args);

        public IDisposable BeginScope(string messageFormat, params object[] args)
            => new NoopDisposable();
    }

    public class LoggerConfiguration
    {
        public string LogFilePath { get; set; } = "logs/log-.txt";
        public int RetentionDays { get; set; } = 31;
        public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Debug;
    }

    internal class NoopDisposable : IDisposable
    {
        public void Dispose() { }
    }
}