```csharp
using System;
using System.Windows.Forms;
using PC1MAINAITradingSystem.Core.DataMigration;

namespace PC1MAINAITradingSystem.Forms
{
    public partial class MainForm
    {
        private void InitializeDataMigrationComponents()
        {
            var planMigrationMenuItem = new ToolStripMenuItem("Plan Migration", null, OnPlanMigration);
            var executeMigrationMenuItem = new ToolStripMenuItem("Execute Migration", null, OnExecuteMigration);
            var viewHistoryMenuItem = new ToolStripMenuItem("View Migration History", null, OnViewMigrationHistory);

            migrationMenu.DropDownItems.AddRange(new ToolStripItem[]
            {
                planMigrationMenuItem,
                executeMigrationMenuItem,
                viewHistoryMenuItem
            });
        }

        private void OnPlanMigration(object sender, EventArgs e)
        {
            try
            {
                if (!_erdService.HasValidERD())
                {
                    MessageBox.Show("Please load and validate an ERD first.", "Notice",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var migrationPlan = _migrationService.CreateMigrationPlan();
                using (var planViewer = new MigrationPlanViewer(migrationPlan))
                {
                    if (planViewer.ShowDialog() == DialogResult.OK)
                    {
                        _logger.Log("Migration plan created and saved");
                        MessageBox.Show("Migration plan has been created.", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating migration plan: {ex.Message}");
                MessageBox.Show($"Failed to create migration plan: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnExecuteMigration(object sender, EventArgs e)
        {
            try
            {
                if (!_migrationService.HasValidMigrationPlan())
                {
                    MessageBox.Show("Please create a migration plan first.", "Notice",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var result = MessageBox.Show(
                    "Are you sure you want to execute the migration? This process cannot be undone.",
                    "Confirm Migration",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    using (var progress = new MigrationProgressForm(_migrationService))
                    {
                        if (progress.ShowDialog() == DialogResult.OK)
                        {
                            _logger.Log("Migration executed successfully");
                            MessageBox.Show("Migration completed successfully.", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            RefreshDatabaseExplorer();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error executing migration: {ex.Message}");
                MessageBox.Show($"Failed to execute migration: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnViewMigrationHistory(object sender, EventArgs e)
        {
            try
            {
                using (var historyViewer = new MigrationHistoryViewer(_migrationService))
                {
                    historyViewer.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error viewing migration history: {ex.Message}");
                MessageBox.Show($"Failed to view migration history: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
```