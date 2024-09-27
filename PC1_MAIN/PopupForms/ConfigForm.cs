using System;
using System.Windows.Forms;
using System.Data.SqlClient;
using ForexAITradingSystem.Utilities;

namespace ForexAITradingSystem.PopupForms
{
    public partial class ConfigForm : Form
    {
        public Config UpdatedConfig { get; private set; }

        private TextBox txtDbServerIp;
        private TextBox txtDbUserId;
        private TextBox txtDbPassword;
        private Button btnSave;
        private Button btnCancel;
        private Button btnTestConnection;

        public ConfigForm(Config currentConfig)
        {
            UpdatedConfig = currentConfig ?? new Config();
            InitializeComponent();
            LoadCurrentConfig();
        }

        private void InitializeComponent()
        {
            this.txtDbServerIp = new TextBox();
            this.txtDbUserId = new TextBox();
            this.txtDbPassword = new TextBox();
            this.btnSave = new Button();
            this.btnCancel = new Button();
            this.btnTestConnection = new Button();

            // txtDbServerIp
            this.txtDbServerIp.Location = new System.Drawing.Point(120, 20);
            this.txtDbServerIp.Name = "txtDbServerIp";
            this.txtDbServerIp.Size = new System.Drawing.Size(200, 23);

            // txtDbUserId
            this.txtDbUserId.Location = new System.Drawing.Point(120, 60);
            this.txtDbUserId.Name = "txtDbUserId";
            this.txtDbUserId.Size = new System.Drawing.Size(200, 23);

            // txtDbPassword
            this.txtDbPassword.Location = new System.Drawing.Point(120, 100);
            this.txtDbPassword.Name = "txtDbPassword";
            this.txtDbPassword.PasswordChar = '*';
            this.txtDbPassword.Size = new System.Drawing.Size(200, 23);

            // btnSave
            this.btnSave.Location = new System.Drawing.Point(120, 140);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.Text = "Save";
            this.btnSave.Click += new EventHandler(this.btnSave_Click);

            // btnCancel
            this.btnCancel.Location = new System.Drawing.Point(245, 140);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);

            // btnTestConnection
            this.btnTestConnection.Location = new System.Drawing.Point(120, 180);
            this.btnTestConnection.Name = "btnTestConnection";
            this.btnTestConnection.Size = new System.Drawing.Size(200, 23);
            this.btnTestConnection.Text = "Server 접속";
            this.btnTestConnection.Click += new EventHandler(this.btnTestConnection_Click);

            // Labels
            Label lblDbServerIp = new Label();
            lblDbServerIp.Text = "DB Server IP:";
            lblDbServerIp.Location = new System.Drawing.Point(20, 23);

            Label lblDbUserId = new Label();
            lblDbUserId.Text = "DB User ID:";
            lblDbUserId.Location = new System.Drawing.Point(20, 63);

            Label lblDbPassword = new Label();
            lblDbPassword.Text = "DB Password:";
            lblDbPassword.Location = new System.Drawing.Point(20, 103);

            // ConfigForm
            this.ClientSize = new System.Drawing.Size(350, 220);
            this.Controls.AddRange(new Control[] {
                lblDbServerIp, lblDbUserId, lblDbPassword,
                this.txtDbServerIp, this.txtDbUserId, this.txtDbPassword,
                this.btnSave, this.btnCancel, this.btnTestConnection
            });
            this.Name = "ConfigForm";
            this.Text = "Database 접속";
        }

        private void LoadCurrentConfig()
        {
            txtDbServerIp.Text = UpdatedConfig.DbServerIp ?? string.Empty;
            txtDbUserId.Text = UpdatedConfig.DbUserId ?? string.Empty;
            txtDbPassword.Text = UpdatedConfig.DbPassword ?? string.Empty;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            UpdatedConfig.DbServerIp = txtDbServerIp.Text;
            UpdatedConfig.DbUserId = txtDbUserId.Text;
            UpdatedConfig.DbPassword = txtDbPassword.Text;

            ConfigManager.SaveConfig(UpdatedConfig);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            string connectionString = $"Server={txtDbServerIp.Text};Database=master;User Id={txtDbUserId.Text};Password={txtDbPassword.Text};";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    MessageBox.Show("Database connection successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    UpdatedConfig.DbServerIp = txtDbServerIp.Text;
                    UpdatedConfig.DbUserId = txtDbUserId.Text;
                    UpdatedConfig.DbPassword = txtDbPassword.Text;
                    ConfigManager.SaveConfig(UpdatedConfig);

                    // 새로운 DatabaseManagerForm 열기
                    using (var dbManagerForm = new DatabaseManagerForm(connectionString))
                    {
                        dbManagerForm.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessageWithCopy(ex.Message);
            }
        }

        private void ShowErrorMessageWithCopy(string errorMessage)
        {
            using (Form errorForm = new Form())
            {
                errorForm.Text = "Connection Error";
                errorForm.Size = new System.Drawing.Size(400, 200);
                errorForm.StartPosition = FormStartPosition.CenterParent;

                TextBox txtError = new TextBox
                {
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    Dock = DockStyle.Fill,
                    Text = errorMessage
                };

                Button btnCopy = new Button
                {
                    Text = "복사",
                    Dock = DockStyle.Bottom
                };
                btnCopy.Click += (s, e) =>
                {
                    Clipboard.SetText(errorMessage);
                    errorForm.Close();
                };

                errorForm.Controls.Add(txtError);
                errorForm.Controls.Add(btnCopy);

                errorForm.ShowDialog(this);
            }
        }
    }
}