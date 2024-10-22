using System;
using System.Windows.Forms;

namespace PC1MAINAITradingSystem.Utils
{
    public class Logger
    {
        private TextBox _logTextBox;

        public Logger(TextBox logTextBox)
        {
            _logTextBox = logTextBox;
        }

        public void AddLog(string message)
        {
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            if (_logTextBox.InvokeRequired)
            {
                _logTextBox.Invoke(new Action<string>(AppendLogMessage), logMessage);
            }
            else
            {
                AppendLogMessage(logMessage);
            }
        }

        private void AppendLogMessage(string message)
        {
            _logTextBox.AppendText(message + Environment.NewLine);
            _logTextBox.ScrollToCaret();
        }
    }
}