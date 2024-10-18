using System;
using System.IO;
using System.Windows.Forms;

namespace PC1MAINAITradingSystem.Forms
{
    public partial class MainForm
    {
        private void InitializeLogging()
        {
            // 로깅 초기화 로직
        }

        protected void AddLog(string message)
        {
            if (logTextBox.InvokeRequired)
            {
                logTextBox.Invoke(new Action<string>(AddLog), message);
            }
            else
            {
                logTextBox.AppendText($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
                logTextBox.ScrollToCaret();
            }
        }

        private void SaveLogToFile()
        {
            try
            {
                string logFilePath = Path.Combine(Application.StartupPath, "application.log");
                File.AppendAllText(logFilePath, logTextBox.Text);
                MessageBox.Show("Log saved successfully.", "Log Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving log: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearLog()
        {
            logTextBox.Clear();
            AddLog("Log cleared.");
        }

        // 추가적인 로깅 관련 메서드들...
    }
}