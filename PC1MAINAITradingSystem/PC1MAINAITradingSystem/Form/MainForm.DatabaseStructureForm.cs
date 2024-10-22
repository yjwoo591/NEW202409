using System;
using System.Windows.Forms;
using PC1MAINAITradingSystem.Database;

namespace PC1MAINAITradingSystem.Forms
{
    public class DatabaseStructureForm : Form
    {
        private DatabaseManager _dbManager;
        private TreeView treeViewStructure;

        public DatabaseStructureForm(DatabaseManager dbManager)
        {
            _dbManager = dbManager;
            InitializeComponent();
            LoadDatabaseStructure();
        }

        private void InitializeComponent()
        {
            this.Text = "Database Structure";
            this.Size = new System.Drawing.Size(600, 400);

            treeViewStructure = new TreeView
            {
                Dock = DockStyle.Fill
            };

            this.Controls.Add(treeViewStructure);
        }

        private void LoadDatabaseStructure()
        {
            var tables = _dbManager.GetTables();
            foreach (var table in tables)
            {
                TreeNode tableNode = new TreeNode(table.Name);
                foreach (var column in table.Columns)
                {
                    tableNode.Nodes.Add(new TreeNode($"{column.Name} ({column.Type})"));
                }
                treeViewStructure.Nodes.Add(tableNode);
            }
        }
    }
}