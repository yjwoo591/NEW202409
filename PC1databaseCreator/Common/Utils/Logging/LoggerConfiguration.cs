using Serilog;
using Serilog.Events;

namespace PC1databaseCreator.Common.Utils.Logging
{
    /// <summary>
    /// 로깅 설정을 구성하는 클래스
    /// </summary>
    public static class LoggerConfiguration
    {
        public static ILogger CreateLogger()
        {
            return new Serilog.LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.File("logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }
    }
}