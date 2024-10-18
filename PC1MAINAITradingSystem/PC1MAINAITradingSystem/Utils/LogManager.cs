using System;
using System.IO;

namespace PC1MAINAITradingSystem.Utils
{
    public class LogManager
    {
        private string _logFilePath;

        public LogManager(string logFilePath = "application.log")
        {
            _logFilePath = logFilePath;
        }

        public void Log(string message)
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
            File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
        }

        // 추가적인 로깅 메서드들...
    }
}