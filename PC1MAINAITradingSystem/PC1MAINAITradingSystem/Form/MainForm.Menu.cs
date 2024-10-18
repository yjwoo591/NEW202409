using System;
using System.Windows.Forms;

namespace PC1MAINAITradingSystem.Forms
{
    public partial class MainForm
    {
        private void CreateMainMenu()
        {
            mainMenuStrip = new MenuStrip();

            // File menu
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
            fileMenu.DropDownItems.Add("Exit", null, (sender, e) => Application.Exit());

            // Database menu
            ToolStripMenuItem databaseMenu = new ToolStripMenuItem("Database");
            databaseMenu.DropDownItems.Add("Connect", null, OnDatabaseConnect);
            databaseMenu.DropDownItems.Add("Disconnect", null, OnDatabaseDisconnect);
            databaseMenu.DropDownItems.Add("View Database and Tables", null, OnSelectDatabase);

            // ERD menu
            ToolStripMenuItem erdMenu = new ToolStripMenuItem("ERD");
            erdMenu.DropDownItems.Add("Generate Database from ERD", null, OnGenerateDatabaseFromERD);
            erdMenu.DropDownItems.Add("Read ERD", null, OnReadERD);
            erdMenu.DropDownItems.Add("Save ERD", null, OnSaveERD);
            erdMenu.DropDownItems.Add("Backup ERD", null, OnBackupERD);

            mainMenuStrip.Items.Add(fileMenu);
            mainMenuStrip.Items.Add(databaseMenu);
            mainMenuStrip.Items.Add(erdMenu);

            this.MainMenuStrip = mainMenuStrip;
            this.Controls.Add(mainMenuStrip);
        }

        // 메뉴 이벤트 핸들러 메서드들...
    }
}