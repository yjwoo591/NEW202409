using System;
using System.Windows.Forms;
using PC1MAINAITradingSystem.Database;
using PC1MAINAITradingSystem.Utils;

namespace PC1MAINAITradingSystem.Forms
{
    public partial class MainForm : Form
    {
        protected DatabaseManager _dbManager;
        protected ERDManager _erdManager;
        protected LogManager _logManager;

        // UI components (declared here but initialized in MainForm.UI.cs)
        protected MenuStrip mainMenuStrip;
        protected StatusStrip statusStrip;
        protected ToolStripStatusLabel statusLabel;
        protected SplitContainer splitContainer;
        protected Panel mainPanel;
        protected TextBox logTextBox;

        public MainForm()
        {
            _dbManager = new DatabaseManager();
            _erdManager = new ERDManager();
            _logManager = new LogManager();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form settings
            this.Text = "PC1 MAIN AI Trading System";
            this.Size = new System.Drawing.Size(1024, 768);
            this.StartPosition = FormStartPosition.CenterScreen;

            InitializeMenu();
            InitializeUI();
            InitializeDatabaseComponents();
            InitializeERDComponents();

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // Method declarations for partial methods implemented in other files
        partial void InitializeMenu();
        partial void InitializeUI();
        partial void InitializeDatabaseComponents();
        partial void InitializeERDComponents();

        // Common methods used across partial classes
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

        protected void UpdateStatus(string message)
        {
            if (statusLabel.Owner.InvokeRequired)
            {
                statusLabel.Owner.Invoke(new Action<string>(UpdateStatus), message);
            }
            else
            {
                statusLabel.Text = message;
            }
        }

        // Event handler for form closing
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _dbManager.Disconnect();
            AddLog("Application closing");
        }
    }
}