using System;
using System.Windows.Forms;

namespace PC1MAINAITradingSystem.Forms
{
    public partial class MainForm
    {
        partial void InitializeMenu()
        {
            MenuStrip mainMenu = new MenuStrip();
            this.MainMenuStrip = mainMenu;

            // File Menu
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
            fileMenu.DropDownItems.Add("Exit", null, OnExit);
            mainMenu.Items.Add(fileMenu);

            // Database Menu
            ToolStripMenuItem databaseMenu = new ToolStripMenuItem("Database");
            databaseMenu.DropDownItems.Add("Connect", null, OnDatabaseConnect);
            databaseMenu.DropDownItems.Add("Disconnect", null, OnDatabaseDisconnect);
            databaseMenu.DropDownItems.Add("View Structure", null, OnViewDatabaseStructure);
            mainMenu.Items.Add(databaseMenu);

            // ERD Menu
            ToolStripMenuItem erdMenu = new ToolStripMenuItem("ERD");
            erdMenu.DropDownItems.Add("Load ERD", null, OnLoadERD);
            erdMenu.DropDownItems.Add("Save ERD", null, OnSaveERD);
            mainMenu.Items.Add(erdMenu);

            // Data Transfer Menu
            ToolStripMenuItem dataTransferMenu = new ToolStripMenuItem("Data Transfer");
            dataTransferMenu.DropDownItems.Add("Transfer to AI", null, OnTransferToAI);
            dataTransferMenu.DropDownItems.Add("Transfer to API", null, OnTransferToAPI);
            mainMenu.Items.Add(dataTransferMenu);

            this.Controls.Add(mainMenu);
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}