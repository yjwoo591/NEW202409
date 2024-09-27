using System;
using System.Drawing;
using System.Windows.Forms;
using ForexAITradingSystem.Managers;
using ForexAITradingSystem.Models;

namespace ForexAITradingSystem.PopupForms
{
    public partial class ApiConfigForm : Form
    {
        private readonly ApiManager _apiManager;

        private Label lblApiKey;
        private TextBox txtApiKey;
        private Label lblApiSecret;
        private TextBox txtApiSecret;
        private Label lblEndpoint;
        private TextBox txtEndpoint;
        private Button btnSaveConfig;

        public ApiConfigForm(ApiManager apiManager)
        {
            InitializeComponent();
            _apiManager = apiManager;
            CreateControls();
            ArrangeControls();
        }

        private void CreateControls()
        {
            lblApiKey = new Label { Text = "API Key:", AutoSize = true };
            txtApiKey = new TextBox { Width = 200 };

            lblApiSecret = new Label { Text = "API Secret:", AutoSize = true };
            txtApiSecret = new TextBox { Width = 200 };

            lblEndpoint = new Label { Text = "API Endpoint:", AutoSize = true };
            txtEndpoint = new TextBox { Width = 200 };

            btnSaveConfig = new Button { Text = "Save Configuration", AutoSize = true };
            btnSaveConfig.Click += BtnSaveConfig_Click;
        }

        private void ArrangeControls()
        {
            int padding = 10;
            int y = padding;

            lblApiKey.Location = new Point(padding, y);
            y += lblApiKey.Height + padding;
            txtApiKey.Location = new Point(padding, y);
            y += txtApiKey.Height + padding;

            lblApiSecret.Location = new Point(padding, y);
            y += lblApiSecret.Height + padding;
            txtApiSecret.Location = new Point(padding, y);
            y += txtApiSecret.Height + padding;

            lblEndpoint.Location = new Point(padding, y);
            y += lblEndpoint.Height + padding;
            txtEndpoint.Location = new Point(padding, y);
            y += txtEndpoint.Height + padding;

            btnSaveConfig.Location = new Point(padding, y);

            ClientSize = new Size(Math.Max(btnSaveConfig.Right + padding, 300), btnSaveConfig.Bottom + padding);

            Controls.AddRange(new Control[] { lblApiKey, txtApiKey, lblApiSecret, txtApiSecret, lblEndpoint, txtEndpoint, btnSaveConfig });
        }

        private void BtnSaveConfig_Click(object sender, EventArgs e)
        {
            var apiConfig = new ApiConfig
            {
                ApiKey = txtApiKey.Text,
                ApiSecret = txtApiSecret.Text,
                Endpoint = txtEndpoint.Text
            };
            _apiManager.UpdateConfig(apiConfig);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}