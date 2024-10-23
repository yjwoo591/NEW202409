using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Threading.Tasks;
using PC1MAINAITradingSystem.Utils;

namespace PC1MAINAITradingSystem.Forms
{
    public partial class MainForm
    {
        private async Task ShowDatabaseConnection()
        {
            try
            {
                using var form = new DatabaseConnectionForm();
                if (form.ShowDialog() == DialogResult.OK)
                {
                    var connectionString = $"Server={form.Server};Database={form.Database};" +
                                         $"User Id={form.Username};Password={form.Password};";

                    await ConnectDatabase(connectionString);
                }
            }
            catch (Exception ex)
            {
                await _logger.Error("Failed to show database connection dialog", ex);
                MessageBox.Show("데이터베이스 연결 대화상자를 표시하는 중 오류가 발생했습니다.",
                              "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ConnectDatabase(string connectionString)
        {
            try
            {
                var connection = new SqlConnection(connectionString);
                _sqlHelper = new SqlHelper(connection);

                if (await _sqlHelper.TestConnectionAsync())
                {
                    UpdateConnectionStatus(true);
                    await _logger.Info("Database connected successfully");
                }
                else
                {
                    throw new Exception("Connection test failed");
                }
            }
            catch (Exception ex)
            {
                UpdateConnectionStatus(false);
                await _logger.Error("Failed to connect to database", ex);
                MessageBox.Show("데이터베이스 연결에 실패했습니다.",
                              "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DisconnectDatabase()
        {
            try
            {
                if (_sqlHelper != null)
                {
                    await _sqlHelper.ExecuteNonQueryAsync("/* Disconnect */");
                    UpdateConnectionStatus(false);
                    await _logger.Info("Database disconnected");
                }
            }
            catch (Exception ex)
            {
                await _logger.Error("Failed to disconnect from database", ex);
                MessageBox.Show("데이터베이스 연결 해제 중 오류가 발생했습니다.",
                              "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task GenerateERDFromDatabase()
        {
            try
            {
                if (!_isConnected)
                {
                    MessageBox.Show("데이터베이스에 연결되어 있지 않습니다.",
                                  "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await _logger.Info("Generating ERD from database...");
                // ERD 생성 구현
            }
            catch (Exception ex)
            {
                await _logger.Error("Failed to generate ERD from database", ex);
                MessageBox.Show("데이터베이스로부터 ERD를 생성하는 중 오류가 발생했습니다.",
                              "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ApplyERDToDatabase()
        {
            try
            {
                if (!_isConnected)
                {
                    MessageBox.Show("데이터베이스에 연결되어 있지 않습니다.",
                                  "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    "이 작업은 데이터베이스 스키마를 변경합니다. 계속하시겠습니까?",
                    "확인",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    await _logger.Info("Applying ERD to database...");
                    // ERD 적용 구현
                }
            }
            catch (Exception ex)
            {
                await _logger.Error("Failed to apply ERD to database", ex);
                MessageBox.Show("ERD를 데이터베이스에 적용하는 중 오류가 발생했습니다.",
                              "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public class DatabaseConnectionForm : Form
    {
        private TextBox _serverTextBox;
        private TextBox _databaseTextBox;
        private TextBox _usernameTextBox;
        private TextBox _passwordTextBox;

        public string Server => _serverTextBox.Text;
        public string Database => _databaseTextBox.Text;
        public string Username => _usernameTextBox.Text;
        public string Password => _passwordTextBox.Text;

        public DatabaseConnectionForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Text = "데이터베이스 연결";
            Size = new System.Drawing.Size(300, 200);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                RowCount = 5,
                ColumnCount = 2
            };

          



                // 서버
            layout.Controls.Add(new Label { Text = "서버:" }, 0, 0);
                _serverTextBox = new TextBox { Dock = DockStyle.Fill };
                layout.Controls.Add(_serverTextBox, 1, 0);

                // 데이터베이스
                layout.Controls.Add(new Label { Text = "데이터베이스:" }, 0, 1);
                _databaseTextBox = new TextBox { Dock = DockStyle.Fill };
                layout.Controls.Add(_databaseTextBox, 1, 1);

                // 사용자명
                layout.Controls.Add(new Label { Text = "사용자명:" }, 0, 2);
                _usernameTextBox = new TextBox { Dock = DockStyle.Fill };
                layout.Controls.Add(_usernameTextBox, 1, 2);

                // 비밀번호
                layout.Controls.Add(new Label { Text = "비밀번호:" }, 0, 3);
                _passwordTextBox = new TextBox
                {
                    Dock = DockStyle.Fill,
                    UseSystemPasswordChar = true
                };
                layout.Controls.Add(_passwordTextBox, 1, 3);

                // 버튼 패널
                var buttonPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft
                };

                var okButton = new Button
                {
                    Text = "연결",
                    DialogResult = DialogResult.OK
                };
                okButton.Click += (s, e) => ValidateAndClose();

                var cancelButton = new Button
                {
                    Text = "취소",
                    DialogResult = DialogResult.Cancel
                };

                buttonPanel.Controls.AddRange(new Control[] { cancelButton, okButton });
                layout.Controls.Add(buttonPanel, 1, 4);

                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));


            for (int i = 0; i < 5; i++)
            {
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            }

            Controls.Add(layout);
            AcceptButton = okButton;
            CancelButton = cancelButton;
        }

        private void ValidateAndClose()
        {
            if (string.IsNullOrWhiteSpace(Server))
            {
                MessageBox.Show("서버를 입력하세요.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _serverTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(Database))
            {
                MessageBox.Show("데이터베이스를 입력하세요.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _databaseTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(Username))
            {
                MessageBox.Show("사용자명을 입력하세요.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _usernameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("비밀번호를 입력하세요.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _passwordTextBox.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}