using System;
using System.Windows.Forms;
using ForexAITradingPC2.Models;
using ForexAITradingPC2.Utils;

namespace ForexAITradingPC2.Forms
{
    public partial class MainForm : Form
    {
        private DatabaseManager dbManager;

        public MainForm()
        {
            InitializeComponent();
            dbManager = new DatabaseManager();
            if (!dbManager.HasValidConfig())
            {
                ShowDatabaseConfigDialog();
            }
            CreateMenu();
        }

        private void CreateMenu()
        {
            MenuStrip menuStrip = new MenuStrip();
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
            ToolStripMenuItem settingsMenu = new ToolStripMenuItem("Settings");
            ToolStripMenuItem dataMenu = new ToolStripMenuItem("Data");
            ToolStripMenuItem analysisMenu = new ToolStripMenuItem("Analysis");
            ToolStripMenuItem tradingMenu = new ToolStripMenuItem("Trading");
            ToolStripMenuItem helpMenu = new ToolStripMenuItem("Help");

            menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, settingsMenu, dataMenu, analysisMenu, tradingMenu, helpMenu });

            // File menu items
            fileMenu.DropDownItems.Add("Exit", null, ExitApplication);

            // Settings menu items
            settingsMenu.DropDownItems.Add("Database Configuration", null, ShowDatabaseConfigDialog);

            // Data menu items
            dataMenu.DropDownItems.Add("Import Data", null, ImportData);
            dataMenu.DropDownItems.Add("Export Data", null, ExportData);

            // Analysis menu items
            analysisMenu.DropDownItems.Add("Run Analysis", null, RunAnalysis);
            analysisMenu.DropDownItems.Add("View Results", null, ViewResults);

            // Trading menu items
            tradingMenu.DropDownItems.Add("Place Order", null, PlaceOrder);
            tradingMenu.DropDownItems.Add("View Positions", null, ViewPositions);

            // Help menu items
            helpMenu.DropDownItems.Add("About", null, ShowAboutDialog);
        }

        private void ExitApplication(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ShowDatabaseConfigDialog(object sender = null, EventArgs e = null)
        {
            using (var form = new DatabaseConfigForm(dbManager))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    // Configuration saved in the form
                }
                else if (!dbManager.HasValidConfig())
                {
                    MessageBox.Show("Valid database configuration is required to run the application.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }
            }
        }

            private void ImportData(object sender, EventArgs e)
        {
            MessageBox.Show("Import Data functionality to be implemented", "Not Implemented", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExportData(object sender, EventArgs e)
        {
            MessageBox.Show("Export Data functionality to be implemented", "Not Implemented", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void RunAnalysis(object sender, EventArgs e)
        {
            MessageBox.Show("Run Analysis functionality to be implemented", "Not Implemented", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ViewResults(object sender, EventArgs e)
        {
            MessageBox.Show("View Results functionality to be implemented", "Not Implemented", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void PlaceOrder(object sender, EventArgs e)
        {
            MessageBox.Show("Place Order functionality to be implemented", "Not Implemented", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ViewPositions(object sender, EventArgs e)
        {
            MessageBox.Show("View Positions functionality to be implemented", "Not Implemented", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowAboutDialog(object sender, EventArgs e)
        {
            MessageBox.Show("Forex AI Trading System\nVersion 1.0\n© 2024 Your Company", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}