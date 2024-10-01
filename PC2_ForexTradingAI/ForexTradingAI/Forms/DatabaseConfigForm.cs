using System;
using System.Windows.Forms;
using ForexAITradingPC2.Models;
using ForexAITradingPC2.Utils;

namespace ForexAITradingPC2.Forms
{
    public class DatabaseConfigForm : Form
    {
        private TextBox txtServerIP;
        private TextBox txtUserID;
        private TextBox txtPassword;
        private Button btnSave;
        private Button btnTest;
        private Button btnCancel;
        private DatabaseManager dbManager;

        public DatabaseConfigForm(DatabaseManager manager)
        {
            dbManager = manager;
            InitializeComponent();
            LoadExistingConfig();
        }

        private void InitializeComponent()
        {
            this.txtServerIP = new TextBox();
            this.txtUserID = new TextBox();
            this.txtPassword = new TextBox();
            this.btnSave = new Button();
            this.btnTest = new Button();
            this.btnCancel = new Button();

            // ServerIP
            this.txtServerIP.Location = new System.Drawing.Point(120, 20);
            this.txtServerIP.Name = "txtServerIP";
            this.txtServerIP.Size = new System.Drawing.Size(200, 23);

            // UserID
            this.txtUserID.Location = new System.Drawing.Point(120, 50);
            this.txtUserID.Name = "txtUserID";
            this.txtUserID.Size = new System.Drawing.Size(200, 23);

            // Password
            this.txtPassword.Location = new System.Drawing.Point(120, 80);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(200, 23);

            // Test Button
            this.btnTest.Location = new System.Drawing.Point(120, 110);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 23);
            this.btnTest.Text = "Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new EventHandler(BtnTest_Click);

            // Save Button
            this.btnSave.Location = new System.Drawing.Point(200, 110);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new EventHandler(BtnSave_Click);

            // Cancel Button
            this.btnCancel.Location = new System.Drawing.Point(280, 110);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(BtnCancel_Click);

            // Labels
            this.Controls.Add(new Label { Text = "Server IP:", Location = new System.Drawing.Point(20, 23), AutoSize = true });
            this.Controls.Add(new Label { Text = "User ID:", Location = new System.Drawing.Point(20, 53), AutoSize = true });
            this.Controls.Add(new Label { Text = "Password:", Location = new System.Drawing.Point(20, 83), AutoSize = true });

            // Form
            this.ClientSize = new System.Drawing.Size(380, 150);
            this.Controls.Add(this.txtServerIP);
            this.Controls.Add(this.txtUserID);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancel);
            this.Name = "DatabaseConfigForm";
            this.Text = "Database Configuration";
            this.FormClosing += new FormClosingEventHandler(DatabaseConfigForm_FormClosing);
        }

        private void LoadExistingConfig()
        {
            var config = dbManager.GetConfig();
            if (config != null)
            {
                txtServerIP.Text = config.ServerIP;
                txtUserID.Text = config.UserID;
                txtPassword.Text = config.Password;
            }
        }

        private void BtnTest_Click(object sender, EventArgs e)
        {
            var config = GetDatabaseConfig();
            dbManager.SaveConfig(config);
            dbManager.InitializeDatabase();

            if (dbManager.TestConnection())
            {
                MessageBox.Show("Connection successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Connection failed. Please check your settings.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            var config = GetDatabaseConfig();
            dbManager.SaveConfig(config);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void DatabaseConfigForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult != DialogResult.OK && !dbManager.HasValidConfig())
            {
                MessageBox.Show("You must enter valid database configuration to continue.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
            }
        }

        private DatabaseConfig GetDatabaseConfig()
        {
            return new DatabaseConfig
            {
                ServerIP = txtServerIP.Text,
                UserID = txtUserID.Text,
                Password = txtPassword.Text
            };
        }
    }
}