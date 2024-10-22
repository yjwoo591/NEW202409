using System;
using System.Windows.Forms;
using System.Collections.Generic;
using PC1MAINAITradingSystem.Database;
using PC1MAINAITradingSystem.Data;

namespace PC1MAINAITradingSystem.Forms
{
    public class DataTransferForm : Form
    {
        private readonly DatabaseManager _dbManager;
        private readonly AIDataTransfer _dataTransfer;

        // UI Controls
        private ComboBox _sourceDbComboBox;
        private ComboBox _targetDbComboBox;
        private CheckedListBox _tablesCheckedListBox;
        private Button _transferButton;
        private Button _cancelButton;

        public DataTransferForm(DatabaseManager dbManager, AIDataTransfer dataTransfer)
        {
            _dbManager = dbManager;
            _dataTransfer = dataTransfer;
            InitializeComponent();
            LoadDatabases();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form settings
            this.Text = "Data Transfer";
            this.Size = new System.Drawing.Size(400, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Source Database ComboBox
            _sourceDbComboBox = new ComboBox
            {
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(200, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _sourceDbComboBox.SelectedIndexChanged += SourceDb_SelectedIndexChanged;

            // Target Database ComboBox
            _targetDbComboBox = new ComboBox
            {
                Location = new System.Drawing.Point(20, 60),
                Size = new System.Drawing.Size(200, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Tables CheckedListBox
            _tablesCheckedListBox = new CheckedListBox
            {
                Location = new System.Drawing.Point(20, 100),
                Size = new System.Drawing.Size(340, 280),
                CheckOnClick = true
            };

            // Transfer Button
            _transferButton = new Button
            {
                Text = "Transfer",
                Location = new System.Drawing.Point(180, 400),
                Size = new System.Drawing.Size(80, 30),
                DialogResult = DialogResult.OK
            };
            _transferButton.Click += TransferButton_Click;

            // Cancel Button
            _cancelButton = new Button
            {
                Text = "Cancel",
                Location = new System.Drawing.Point(280, 400),
                Size = new System.Drawing.Size(80, 30),
                DialogResult = DialogResult.Cancel
            };

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                _sourceDbComboBox,
                _targetDbComboBox,
                _tablesCheckedListBox,
                _transferButton,
                _cancelButton
            });

            this.ResumeLayout(false);
        }

        private void LoadDatabases()
        {
            List<string> databases = _dbManager.GetDatabases();
            _sourceDbComboBox.Items.AddRange(databases.ToArray());
            _targetDbComboBox.Items.AddRange(databases.ToArray());
        }

        private void SourceDb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_sourceDbComboBox.SelectedItem != null)
            {
                LoadTables(_sourceDbComboBox.SelectedItem.ToString());
            }
        }

        private void LoadTables(string databaseName)
        {
            _tablesCheckedListBox.Items.Clear();
            // Implement loading tables from selected database
            // You'll need to add a method to DatabaseManager to get tables
        }

        private void TransferButton_Click(object sender, EventArgs e)
        {
            if (_sourceDbComboBox.SelectedItem == null || _targetDbComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select both source and target databases.",
                    "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_tablesCheckedListBox.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one table to transfer.",
                    "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _dataTransfer.SourceDatabase = _sourceDbComboBox.SelectedItem.ToString();
            _dataTransfer.TargetDatabase = _targetDbComboBox.SelectedItem.ToString();
            _dataTransfer.ClearTables();

            foreach (var item in _tablesCheckedListBox.CheckedItems)
            {
                _dataTransfer.AddTable(item.ToString());
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}