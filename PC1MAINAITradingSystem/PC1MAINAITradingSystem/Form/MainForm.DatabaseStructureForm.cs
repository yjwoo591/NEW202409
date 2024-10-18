using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using PC1MAINAITradingSystem.Database;

namespace PC1MAINAITradingSystem.Forms
{
    public class DatabaseStructureForm : Form
    {
        private TreeView structureTreeView;
        private Button saveButton;
        private Button cancelButton;
        private List<Table> tables;

        public DatabaseStructureForm(List<Table> tables)
        {
            this.tables = tables;
            InitializeComponent();
            PopulateTreeView();
        }

        private void InitializeComponent()
        {
            this.Text = "Database Structure";
            this.Size = new Size(600, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;

            structureTreeView = new TreeView
            {
                Dock = DockStyle.Fill,
                Location = new Point(12, 12),
                Size = new Size(560, 300),
                CheckBoxes = true
            };

            saveButton = new Button
            {
                Text = "Save Changes",
                Location = new Point(400, 320),
                Size = new Size(100, 30)
            };
            saveButton.Click += SaveButton_Click;

            cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(510, 320),
                Size = new Size(60, 30)
            };
            cancelButton.Click += (sender, e) => this.Close();

            this.Controls.Add(structureTreeView);
            this.Controls.Add(saveButton);
            this.Controls.Add(cancelButton);
        }

        private void PopulateTreeView()
        {
            structureTreeView.Nodes.Clear();
            foreach (var table in tables)
            {
                TreeNode tableNode = new TreeNode(table.Name);
                foreach (var column in table.Columns)
                {
                    TreeNode columnNode = new TreeNode($"{column.Name} ({column.Type}) {column.Constraint}");
                    columnNode.Tag = column;
                    tableNode.Nodes.Add(columnNode);
                }
                structureTreeView.Nodes.Add(tableNode);
            }
            structureTreeView.ExpandAll();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            UpdateTablesFromTreeView();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void UpdateTablesFromTreeView()
        {
            tables.Clear();
            foreach (TreeNode tableNode in structureTreeView.Nodes)
            {
                Table table = new Table { Name = tableNode.Text };
                foreach (TreeNode columnNode in tableNode.Nodes)
                {
                    Column column = (Column)columnNode.Tag;
                    table.Columns.Add(column);
                }
                tables.Add(table);
            }
        }

        public List<Table> GetUpdatedTables()
        {
            return tables;
        }
    }
}