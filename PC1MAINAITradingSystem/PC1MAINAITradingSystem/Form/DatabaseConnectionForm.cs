using System;
using System.Windows.Forms;

namespace PC1MAINAITradingSystem.Forms
{
    public class DatabaseConnectionForm : Form
    {
        private TextBox serverTextBox;
        private TextBox usernameTextBox;
        private TextBox passwordTextBox;
        private Button connectButton;

        public string Server { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }

        public DatabaseConnectionForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Database Connection";
            this.Size = new System.Drawing.Size(300, 200);

            Label serverLabel = new Label { Text = "Server:", Location = new System.Drawing.Point(10, 10) };
            serverTextBox = new TextBox { Location = new System.Drawing.Point(100, 10), Width = 180 };

            Label usernameLabel = new Label { Text = "Username:", Location = new System.Drawing.Point(10, 40) };
            usernameTextBox = new TextBox { Location = new System.Drawing.Point(100, 40), Width = 180 };

            Label passwordLabel = new Label { Text = "Password:", Location = new System.Drawing.Point(10, 70) };
            passwordTextBox = new TextBox { Location = new System.Drawing.Point(100, 70), Width = 180, PasswordChar = '*' };

            connectButton = new Button { Text = "Connect", Location = new System.Drawing.Point(100, 100) };
            connectButton.Click += ConnectButton_Click;

            this.Controls.AddRange(new Control[] { serverLabel, serverTextBox, usernameLabel, usernameTextBox, passwordLabel, passwordTextBox, connectButton });
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            Server = serverTextBox.Text;
            Username = usernameTextBox.Text;
            Password = passwordTextBox.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}