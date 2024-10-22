```csharp
using System;
using System.Windows.Forms;
using PC1MAINAITradingSystem.Core.DatabaseManager;

namespace PC1MAINAITradingSystem.Forms
{
    public partial class MainForm
    {
        private void InitializeDatabaseComponents()
        {
            var createDBMenuItem = new ToolStripMenuItem("Create Database", null, OnCreateDatabase);
            var modifyDBMenuItem = new ToolStripMenuItem("Modify Database", null, OnModifyDatabase);
            var viewDBMenuItem = new ToolStripMenuItem("View Database Structure", null, OnViewDatabaseStructure);
            var backupDBMenuItem = new ToolStripMenuItem("Backup Database", null, OnBackupDatabase);

            databaseMenu.DropDownItems.AddRange(new ToolStripItem[]
            {
                createDBMenuItem,
                modifyDBMenuItem,
                viewDBMenuItem,
                backupDBMenuItem
            });

            InitializeDatabaseExplorer();
        }

        private void InitializeDatabaseExplorer()
        {
            databaseExplorer.BeforeExpand += DatabaseExplorer_BeforeExpand;
            databaseExplorer.NodeMouseDoubleClick += DatabaseExplorer_NodeMouseDoubleClick;
            RefreshDatabaseExplorer();
        }

        private void OnCreateDatabase(object sender, EventArgs e)
        {
            try
            {
                if (!_erdService.HasValidERD())
                {
                    MessageBox.Show("Please load and validate an ERD first.", "Notice",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var erdStructure = _erdService.GetCurrentERDStructure();
                var result = _databaseService.CreateDatabase(erdStructure);

                if (result.Success)
                {
                    _logger.Log("Database created successfully");
                    MessageBox.Show("Database created successfully.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshDatabaseExplorer();
                }
                else
                {
                    throw new Exception(result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating database: {ex.Message}");
                MessageBox.Show($"Failed to create database: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnModifyDatabase(object sender, EventArgs e)
        {
            try
            {
                if (!_erdService.HasValidERD())
                {
                    MessageBox.Show("Please load and validate an ERD first.", "Notice",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var modifyForm = new DatabaseModificationForm(_databaseService))
                {
                    if (modifyForm.ShowDialog() == DialogResult.OK)
                    {
                        _logger.Log("Database modification completed");
                        RefreshDatabaseExplorer();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error modifying database: {ex.Message}");
                MessageBox.Show($"Failed to modify database: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnViewDatabaseStructure(object sender, EventArgs e)
        {
            try
            {
                using (var structureForm = new DatabaseStructureForm(_databaseService))
                {
                    structureForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error viewing database structure: {ex.Message}");
                MessageBox.Show($"Failed to view database structure: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnBackupDatabase(object sender, EventArgs e)
        {
            try
            {
                var result = _databaseService.BackupDatabase();
                if (result.Success)
                {
                    _logger.Log($"Database backup created at: {result.BackupPath}");
                    MessageBox.Show("Database backup created successfully.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    throw new Exception(result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error backing up database: {ex.Message}");
                MessageBox.Show($"Failed to backup database: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshDatabaseExplorer()
        {
            try
            {
                databaseExplorer.Nodes.Clear();
                var databases = _databaseService.GetAllDatabases();

                foreach (var db in databases)
                {
                    var dbNode = new TreeNode(db.Name)
                    {
                        Tag = db,
                        ImageIndex = 0
                    };
                    dbNode.Nodes.Add(new TreeNode("Loading..."));
                    databaseExplorer.Nodes.Add(dbNode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error refreshing database explorer: {ex.Message}");
            }
        }

        private void DatabaseExplorer_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            try
            {
                var node = e.Node;
                if (node.FirstNode?.Text == "Loading...")
                {
                    node.Nodes.Clear();
                    var database = (DatabaseInfo)node.Tag;
                    var tables = _databaseService.GetDatabaseTables(database.Name);

                    foreach (var table in tables)
                    {
                        var tableNode = new TreeNode(table.Name)
                        {
                            Tag = table,
                            ImageIndex = 1
                        };
                        node.Nodes.Add(tableNode);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error expanding database node: {ex.Message}");
            }
        }

        private void DatabaseExplorer_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                var node = e.Node;
                if (node.Tag is TableInfo tableInfo)
                {
                    using (var tableViewer = new TableContentViewer(_databaseService, tableInfo))
                    {
                        tableViewer.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error opening table viewer: {ex.Message}");
                MessageBox.Show($"Failed to open table viewer: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
```