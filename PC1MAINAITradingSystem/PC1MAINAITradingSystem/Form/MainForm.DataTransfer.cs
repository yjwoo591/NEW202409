using System;
using System.Windows.Forms;
using PC1MAINAITradingSystem.Data;

namespace PC1MAINAITradingSystem.Forms
{
    public partial class MainForm
    {
        private AIDataTransfer _dataTransfer;

        private void InitializeDataTransfer()
        {
            _dataTransfer = new AIDataTransfer();
        }

        private void OnDataTransfer(object sender, EventArgs e)
        {
            if (!_dbManager.IsConnected)
            {
                MessageBox.Show("Please connect to a database first.", "Not Connected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var transferForm = new DataTransferForm(_dbManager, _dataTransfer))
            {
                if (transferForm.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Implement data transfer logic here
                        UpdateStatus($"Data transfer started: {_dataTransfer.SourceDatabase} -> {_dataTransfer.TargetDatabase}");
                        AddLog($"Starting data transfer for {_dataTransfer.Tables.Count} tables");

                        // Example transfer logic
                        foreach (string table in _dataTransfer.Tables)
                        {
                            TransferTable(table);
                        }

                        UpdateStatus("Data transfer completed successfully");
                        AddLog("Data transfer completed");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error during data transfer: {ex.Message}", "Transfer Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        AddLog($"Data transfer error: {ex.Message}");
                    }
                }
            }
        }

        private void TransferTable(string tableName)
        {
            try
            {
                string query = $"SELECT * FROM {tableName}";
                var data = _dbManager.ExecuteQuery(query);
                AddLog($"Transferring table: {tableName} ({data.Rows.Count} rows)");
                // Implement actual table transfer logic here
            }
            catch (Exception ex)
            {
                AddLog($"Error transferring table {tableName}: {ex.Message}");
                throw;
            }
        }
    }
}