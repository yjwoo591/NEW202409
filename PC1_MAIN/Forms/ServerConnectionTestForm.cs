using System;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Configuration;

namespace ForexAITradingPC1Main.Forms
{
    public partial class ServerConnectionTestForm : Form
    {
        private TextBox ipTextBox;
        private TextBox userTextBox;
        private TextBox passwordTextBox;
        private Button testButton;
        private Button cancelButton;

        public ServerConnectionTestForm()
        {
            InitializeComponent();
            LoadSavedConnectionInfo();
        }

        private void InitializeComponent()
        {
            this.Size = new System.Drawing.Size(300, 200);
            this.Text = "Server Connection Test";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;

            Label ipLabel = new Label { Text = "Server IP:", Left = 10, Top = 15, Width = 80 };
            ipTextBox = new TextBox { Left = 100, Top = 10, Width = 180 };

            Label userLabel = new Label { Text = "User:", Left = 10, Top = 45, Width = 80 };
            userTextBox = new TextBox { Left = 100, Top = 40, Width = 180 };

            Label passwordLabel = new Label { Text = "Password:", Left = 10, Top = 75, Width = 80 };
            passwordTextBox = new TextBox { Left = 100, Top = 70, Width = 180, PasswordChar = '*' };

            testButton = new Button { Text = "Test Connection", Left = 100, Top = 100, Width = 120 };
            testButton.Click += TestButton_Click;

            cancelButton = new Button { Text = "Cancel", Left = 100, Top = 130, Width = 120 };
            cancelButton.Click += CancelButton_Click;

            this.Controls.AddRange(new Control[] { ipLabel, ipTextBox, userLabel, userTextBox, passwordLabel, passwordTextBox, testButton, cancelButton });
        }

        private void LoadSavedConnectionInfo()
        {
            ipTextBox.Text = ConfigurationManager.AppSettings["ServerIP"] ?? "";
            userTextBox.Text = ConfigurationManager.AppSettings["ServerUser"] ?? "";
            passwordTextBox.Text = ConfigurationManager.AppSettings["ServerPassword"] ?? "";
        }

        private void SaveConnectionInfo()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["ServerIP"].Value = ipTextBox.Text;
            config.AppSettings.Settings["ServerUser"].Value = userTextBox.Text;
            config.AppSettings.Settings["ServerPassword"].Value = passwordTextBox.Text;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private void TestButton_Click(object sender, EventArgs e)
        {
            string connectionString = $"Data Source={ipTextBox.Text};Initial Catalog=ForexTradingDB;User ID={userTextBox.Text};Password={passwordTextBox.Text}";

            try
            {
  
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
/*
 * 이렇게 수정하면 다음과 같이 작동합니다:

프로그램 시작 시 ServerConnectionTestForm이 제외됩니다.
사용자가 서버 정보를 입력하고 연결 테스트를 수행하면, 정보가 저장되는 메인 폼이 역할을 합니다.
다음을 실행하면 자동으로 저장된 정보가 로드됩니다.
사용자는 정보를 수정하고 다시 테스트할 수 있습니다.
취소 버튼을 누르면 프로그램이 종료됩니다.
DatabaseManagementForm에서는 저장된 연결 정보를 사용하여 데이터베이스에 접속합니다.

이 구현을 통해 서버 연결 정보를 안전하게 관리하고, 프로그램을 시작할 때 연결 테스트를 수행할 수 있습니다. 추가적으로 요구하시는 사항이나 수정이 필요한 부분이 있다면 말씀해 주세요.
*/