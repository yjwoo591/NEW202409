```csharp
using System;
using System.Windows.Forms;
using PC1MAINAITradingSystem.Core.ERDProcessor;
using PC1MAINAITradingSystem.Core.DatabaseManager;
using PC1MAINAITradingSystem.Services;
using PC1MAINAITradingSystem.Utils;
using PC1MAINAITradingSystem.Interfaces;
using PC1MAINAITradingSystem.Database;

namespace PC1MAINAITradingSystem.Forms
{
    public partial class MainForm : Form
    {
        // Services
        private readonly IERDService _erdService;
        private readonly IDatabaseService _databaseService;
        private readonly IMigrationService _migrationService;
        private readonly IHistoricalDataService _historicalDataService;
        private readonly ILogger _logger;

        // Core Processors
        private readonly IERDProcessor _erdProcessor;
        private readonly IDatabaseManager _databaseManager;

        public MainForm()
        {
            InitializeComponent();
            InitializeServices();
            InitializeEventHandlers();

            _logger.Log("Application started successfully");
        }

        private void InitializeServices()
        {
            _logger = new Logger();
            _erdService = new ERDService(_logger);
            _databaseService = new DatabaseService(_logger);
            _migrationService = new MigrationService(_logger);
            _historicalDataService = new HistoricalDataService(_logger);

            _erdProcessor = new ERDProcessor(_logger);
            _databaseManager = new DatabaseManager(_logger);

            _logger.Log("Services initialized");
        }

        private void InitializeEventHandlers()
        {
            this.FormClosing += MainForm_FormClosing;
            // 기타 이벤트 핸들러 초기화
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                _databaseService.CloseAllConnections();
                _logger.Log("Application shutting down properly");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during shutdown: {ex.Message}");
                MessageBox.Show("Error occurred during shutdown", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _logger?.Dispose();
                // 다른 리소스 해제
            }
            base.Dispose(disposing);
        }
    }
}
```