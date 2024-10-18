using System;
using System.Windows.Forms;
using System.Collections.Generic;
using PC1MAINAITradingSystem.Database;

namespace PC1MAINAITradingSystem.Forms
{
    public partial class MainForm
    {
        private void InitializeDatabaseComponents()
        {
            // 데이터베이스 관련 UI 컴포넌트 초기화
        }

        private void OnDatabaseConnect(object sender, EventArgs e)
        {
            using (var connectForm = new DatabaseConnectionForm(_dbManager))
            {
                if (connectForm.ShowDialog() == DialogResult.OK)
                {
                    UpdateStatus("Connected to database server");
                    AddLog("Database server connected");
                }
            }
        }

        private void OnDatabaseDisconnect(object sender, EventArgs e)
        {
            _dbManager.Disconnect();
            UpdateStatus("Disconnected from database server");
            AddLog("Database server disconnected");
        }

        private void OnSelectDatabase(object sender, EventArgs e)
        {
            if (_dbManager.IsConnected)
            {
                List<string> databases = _dbManager.GetDatabases();
                using (var selectForm = new DatabaseSelectForm(databases))
                {
                    if (selectForm.ShowDialog() == DialogResult.OK)
                    {
                        string selectedDatabase = selectForm.SelectedDatabase;
                        if (_dbManager.SelectDatabase(selectedDatabase))
                        {
                            UpdateStatus($"Selected database: {selectedDatabase}");
                            AddLog($"Database selected: {selectedDatabase}");
                        }
                        else
                        {
                            MessageBox.Show($"Failed to select database: {selectedDatabase}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please connect to a database server first.", "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // 추가적인 데이터베이스 관련 메서드들...
    }
}