using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;
using Newtonsoft.Json;
using PC1MAINAITradingSystem.Database;

namespace PC1MAINAITradingSystem.Forms
{
    public class DatabaseConnectionForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.TextBox serverTextBox;
        private System.Windows.Forms.TextBox usernameTextBox;
        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.ComboBox savedConnectionsComboBox;

        private DatabaseManager dbManager;
        private List<SavedConnection> savedConnections;

        private static readonly string CONNECTION_FILE_PATH = System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
            "saved_connections.json"
        );

        public DatabaseConnectionForm(DatabaseManager dbManager)
        {
            this.dbManager = dbManager;
            this.savedConnections = LoadSavedConnections();
            InitializeComponent();
            PopulateSavedConnections();
        }

        private void InitializeComponent()
        {
            this.Text = "Database Connection";
            this.Size = new System.Drawing.Size(300, 250);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            this.savedConnectionsComboBox = new System.Windows.Forms.ComboBox
            {
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(260, 20),
                DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            };
            this.savedConnectionsComboBox.SelectedIndexChanged += SavedConnectionsComboBox_SelectedIndexChanged;

            System.Windows.Forms.Label serverLabel = new System.Windows.Forms.Label
            {
                Text = "Server:",
                Location = new System.Drawing.Point(10, 40),
                Size = new System.Drawing.Size(80, 20)
            };
            this.serverTextBox = new System.Windows.Forms.TextBox
            {
                Location = new System.Drawing.Point(100, 40),
                Size = new System.Drawing.Size(180, 20)
            };

            System.Windows.Forms.Label usernameLabel = new System.Windows.Forms.Label
            {
                Text = "Username:",
                Location = new System.Drawing.Point(10, 70),
                Size = new System.Drawing.Size(80, 20)
            };
            this.usernameTextBox = new System.Windows.Forms.TextBox
            {
                Location = new System.Drawing.Point(100, 70),
                Size = new System.Drawing.Size(180, 20)
            };

            System.Windows.Forms.Label passwordLabel = new System.Windows.Forms.Label
            {
                Text = "Password:",
                Location = new System.Drawing.Point(10, 100),
                Size = new System.Drawing.Size(80, 20)
            };
            this.passwordTextBox = new System.Windows.Forms.TextBox
            {
                Location = new System.Drawing.Point(100, 100),
                Size = new System.Drawing.Size(180, 20),
                PasswordChar = '*'
            };

            this.connectButton = new System.Windows.Forms.Button
            {
                Text = "Connect",
                Location = new System.Drawing.Point(100, 140),
                Size = new System.Drawing.Size(75, 23)
            };
            this.connectButton.Click += ConnectButton_Click;

            this.cancelButton = new System.Windows.Forms.Button
            {
                Text = "Cancel",
                Location = new System.Drawing.Point(190, 140),
                Size = new System.Drawing.Size(75, 23)
            };
            this.cancelButton.Click += (sender, e) => this.Close();

            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.savedConnectionsComboBox,
                serverLabel,
                this.serverTextBox,
                usernameLabel,
                this.usernameTextBox,
                passwordLabel,
                this.passwordTextBox,
                this.connectButton,
                this.cancelButton
            });
        }

        private void ConnectButton_Click(object sender, System.EventArgs e)
        {
            string connectionString = $"Server={this.serverTextBox.Text};User Id={this.usernameTextBox.Text};Password={this.passwordTextBox.Text};TrustServerCertificate=True;";
            try
            {
                this.dbManager.ConnectToDatabase("default", connectionString);
                SaveConnection(this.serverTextBox.Text, this.usernameTextBox.Text, this.passwordTextBox.Text);
                System.Windows.Forms.MessageBox.Show("Connection successful!", "Success", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Connection failed: {ex.Message}", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        private void SavedConnectionsComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (this.savedConnectionsComboBox.SelectedIndex > 0)
            {
                SavedConnection selectedConnection = this.savedConnections[this.savedConnectionsComboBox.SelectedIndex - 1];
                this.serverTextBox.Text = selectedConnection.Server;
                this.usernameTextBox.Text = selectedConnection.Username;
                this.passwordTextBox.Text = DecryptPassword(selectedConnection.EncryptedPassword);
            }
        }

        private void PopulateSavedConnections()
        {
            this.savedConnectionsComboBox.Items.Add("Select a saved connection");
            foreach (SavedConnection connection in this.savedConnections)
            {
                this.savedConnectionsComboBox.Items.Add($"{connection.Server} - {connection.Username}");
            }
            this.savedConnectionsComboBox.SelectedIndex = 0;
        }

        private void SaveConnection(string server, string username, string password)
        {
            SavedConnection existingConnection = this.savedConnections.Find(c => c.Server == server && c.Username == username);
            if (existingConnection != null)
            {
                existingConnection.EncryptedPassword = EncryptPassword(password);
            }
            else
            {
                this.savedConnections.Add(new SavedConnection
                {
                    Server = server,
                    Username = username,
                    EncryptedPassword = EncryptPassword(password)
                });
            }
            SaveConnections();
        }

        private List<SavedConnection> LoadSavedConnections()
        {
            try
            {
                if (System.IO.File.Exists(CONNECTION_FILE_PATH))
                {
                    string json = System.IO.File.ReadAllText(CONNECTION_FILE_PATH);
                    List<SavedConnection> connections = JsonConvert.DeserializeObject<List<SavedConnection>>(json);
                    System.Console.WriteLine($"Loaded {connections?.Count ?? 0} saved connections from {CONNECTION_FILE_PATH}");
                    return connections ?? new List<SavedConnection>();
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Error loading saved connections from {CONNECTION_FILE_PATH}: {ex.Message}");
                System.Windows.Forms.MessageBox.Show($"Failed to load saved connections: {ex.Message}", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            return new List<SavedConnection>();
        }

        private void SaveConnections()
        {
            try
            {
                string json = JsonConvert.SerializeObject(this.savedConnections);
                System.IO.File.WriteAllText(CONNECTION_FILE_PATH, json);
                System.Console.WriteLine($"Saved {this.savedConnections.Count} connections to {CONNECTION_FILE_PATH}");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Error saving connections to {CONNECTION_FILE_PATH}: {ex.Message}");
                System.Windows.Forms.MessageBox.Show($"Failed to save connection information: {ex.Message}", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        private string EncryptPassword(string password)
        {
            byte[] encryptedBytes;
            using (System.Security.Cryptography.Aes aes = System.Security.Cryptography.Aes.Create())
            {
                aes.Key = System.Text.Encoding.UTF8.GetBytes("YourSecretKey123YourSecretKey123");
                aes.IV = new byte[16];
                System.Security.Cryptography.ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    using (System.Security.Cryptography.CryptoStream cs = new System.Security.Cryptography.CryptoStream(ms, encryptor, System.Security.Cryptography.CryptoStreamMode.Write))
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(cs))
                    {
                        sw.Write(password);
                    }
                    encryptedBytes = ms.ToArray();
                }
            }
            return System.Convert.ToBase64String(encryptedBytes);
        }

        private string DecryptPassword(string encryptedPassword)
        {
            byte[] encryptedBytes = System.Convert.FromBase64String(encryptedPassword);
            string decryptedPassword;
            using (System.Security.Cryptography.Aes aes = System.Security.Cryptography.Aes.Create())
            {
                aes.Key = System.Text.Encoding.UTF8.GetBytes("YourSecretKey123YourSecretKey123");
                aes.IV = new byte[16];
                System.Security.Cryptography.ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(encryptedBytes))
                using (System.Security.Cryptography.CryptoStream cs = new System.Security.Cryptography.CryptoStream(ms, decryptor, System.Security.Cryptography.CryptoStreamMode.Read))
                using (System.IO.StreamReader sr = new System.IO.StreamReader(cs))
                {
                    decryptedPassword = sr.ReadToEnd();
                }
            }
            return decryptedPassword;
        }
    }

    public class SavedConnection
    {
        public string Server { get; set; }
        public string Username { get; set; }
        public string EncryptedPassword { get; set; }
    }
}