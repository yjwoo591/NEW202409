using System;
using System.IO;
using System.Threading.Tasks;

namespace PC1MAINAITradingSystem.Utils
{
    public sealed class Logger
    {
        private static readonly Lazy<Logger> _instance = new Lazy<Logger>(() => new Logger());
        private readonly string _logPath;
        private const int MAX_LOG_LINES = 7;
        private readonly object _lockObj = new object();

        public static Logger Instance => _instance.Value;

        private Logger()
        {
            _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(_logPath);
        }

        public event EventHandler<string> OnLogAdded;

        public async Task Debug(string message)
        {
            await LogMessage("DEBUG", message);
        }

        public async Task Info(string message)
        {
            await LogMessage("INFO", message);
        }

        public async Task Warning(string message)
        {
            await LogMessage("WARNING", message);
        }

        public async Task Error(string message, Exception ex = null)
        {
            string errorMessage = message;
            if (ex != null)
            {
                errorMessage += $"\nException: {ex.Message}\nStackTrace: {ex.StackTrace}";
            }
            await LogMessage("ERROR", errorMessage);
        }

        private async Task LogMessage(string level, string message)
        {
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}";

            string logFile = Path.Combine(_logPath, $"log_{DateTime.Now:yyyyMMdd}.txt");

            lock (_lockObj)
            {
                File.AppendAllText(logFile, logMessage + Environment.NewLine);
            }

            OnLogAdded?.Invoke(this, logMessage);

            // 로그 로테이션 처리
            await ManageLogRotation(logFile);
        }

        private async Task ManageLogRotation(string logFile)
        {
            try
            {
                var lines = await File.ReadAllLinesAsync(logFile);
                if (lines.Length > 10000) // 파일당 최대 로그 라인 수
                {
                    string archiveFile = Path.Combine(_logPath,
                        $"log_{DateTime.Now:yyyyMMdd}_{DateTime.Now:HHmmss}.archive");

                    await File.WriteAllLinesAsync(archiveFile, lines);
                    File.WriteAllText(logFile, string.Empty);
                }
            }
            catch (Exception ex)
            {
                // 로그 로테이션 실패시 콘솔에만 출력
                Console.WriteLine($"Log rotation failed: {ex.Message}");
            }
        }

        public string[] GetRecentLogs()
        {
            try
            {
                string logFile = Path.Combine(_logPath, $"log_{DateTime.Now:yyyyMMdd}.txt");
                if (!File.Exists(logFile))
                {
                    return new string[0];
                }

                var lines = File.ReadAllLines(logFile);
                return lines.Length <= MAX_LOG_LINES
                    ? lines
                    : lines[(lines.Length - MAX_LOG_LINES)..];
            }
            catch
            {
                return new string[0];
            }
        }
    }
}