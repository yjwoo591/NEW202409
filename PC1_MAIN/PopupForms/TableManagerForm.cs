using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace ForexAITradingSystem.PopupForms
{
    public class TableManagerForm : Form
    {
        private ListBox listTables;
        private Button btnCreate;
        private Button btnDelete;
        private RichTextBox txtTableInfo;
        private string connectionString;
        private string databaseName;

        private Dictionary<string, string> suggestedTables = new Dictionary<string, string>
        {
            {"Users", "사용자 정보 저장"},
            {"ApiConfigurations", "API 설정 정보 저장"},
            {"TradingPairs", "거래 가능한 통화쌍 정보"},
            {"MarketData", "실시간 및 과거 시장 데이터 저장"},
            {"Orders", "주문 정보 저장"},
            {"Trades", "실행된 거래 정보 저장"},
            {"Positions", "사용자의 현재 포지션 정보"},
            {"AccountBalances", "사용자 계정 잔고 정보"},
            {"AIModels", "AI 모델 정보 저장"},
            {"AIPerformance", "AI 모델 성능 데이터 저장"},
            {"TradingSignals", "AI가 생성한 트레이딩 신호 저장"},
            {"RiskManagement", "리스크 관리 설정 및 데이터"},
            {"SystemLogs", "시스템 로그 및 오류 정보"},
            {"Backtests", "백테스팅 결과 저장"}
        };

        private List<string> deletedTables = new List<string>();

        public TableManagerForm(string connectionString, string databaseName)
        {
            this.connectionString = connectionString;
            this.databaseName = databaseName;
            InitializeComponent();
            LoadTables();
        }

        private void InitializeComponent()
        {
            this.listTables = new ListBox();
            this.btnCreate = new Button();
            this.btnDelete = new Button();
            this.txtTableInfo = new RichTextBox();

            // listTables
            this.listTables.Dock = DockStyle.Left;
            this.listTables.SelectionMode = SelectionMode.MultiExtended;
            this.listTables.Width = 200;
            this.listTables.SelectedIndexChanged += ListTables_SelectedIndexChanged;

            // btnCreate
            this.btnCreate.Text = "테이블 생성";
            this.btnCreate.Dock = DockStyle.Bottom;
            this.btnCreate.Click += BtnCreate_Click;

            // btnDelete
            this.btnDelete.Text = "테이블 삭제";
            this.btnDelete.Dock = DockStyle.Bottom;
            this.btnDelete.Click += BtnDelete_Click;

            // txtTableInfo
            this.txtTableInfo.Dock = DockStyle.Fill;
            this.txtTableInfo.ReadOnly = true;

            // TableManagerForm
            this.ClientSize = new System.Drawing.Size(600, 400);
            this.Controls.Add(this.txtTableInfo);
            this.Controls.Add(this.listTables);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.btnDelete);
            this.Text = $"Table Manager - {databaseName}";
        }

        private void LoadTables()
        {
            listTables.Items.Clear();
            List<string> existingTables = new List<string>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                connection.ChangeDatabase(databaseName);

                using (SqlCommand command = new SqlCommand(
                    "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'", connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        existingTables.Add(reader["TABLE_NAME"].ToString());
                    }
                }
            }

            foreach (var table in suggestedTables.Keys.Concat(deletedTables).Distinct())
            {
                if (existingTables.Contains(table))
                {
                    listTables.Items.Add(table);
                }
                else
                {
                    listTables.Items.Add($"[생성 가능] {table}");
                }
            }
        }

        private void ListTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listTables.SelectedItem != null)
            {
                string selectedTable = listTables.SelectedItem.ToString();
                if (selectedTable.StartsWith("[생성 가능] "))
                {
                    selectedTable = selectedTable.Substring(11);
                }

                if (suggestedTables.ContainsKey(selectedTable))
                {
                    txtTableInfo.Text = $"테이블 이름: {selectedTable}\n\n기능: {suggestedTables[selectedTable]}";
                }
                else
                {
                    txtTableInfo.Text = $"테이블 이름: {selectedTable}\n\n기능: 사용자 정의 테이블";
                }
            }
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            List<string> tablesToCreate = new List<string>();

            foreach (object item in listTables.SelectedItems)
            {
                string tableName = item.ToString();
                if (tableName.StartsWith("[생성 가능] "))
                {
                    tablesToCreate.Add(tableName.Substring(11));
                }
            }

            foreach (string tableName in tablesToCreate)
            {
                CreateTable(tableName);
                deletedTables.Remove(tableName);
            }

            LoadTables();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("선택한 테이블을 삭제하시겠습니까?", "확인", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                foreach (object item in listTables.SelectedItems)
                {
                    string tableName = item.ToString();
                    if (!tableName.StartsWith("[생성 가능] "))
                    {
                        DeleteTable(tableName);
                        if (!suggestedTables.ContainsKey(tableName))
                        {
                            deletedTables.Add(tableName);
                        }
                    }
                }
                LoadTables();
            }
        }

        private void CreateTable(string tableName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    connection.ChangeDatabase(databaseName);

                    string sql = GetCreateTableSQL(tableName);
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                MessageBox.Show($"테이블 {tableName}이(가) 생성되었습니다.", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"테이블 생성 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteTable(string tableName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    connection.ChangeDatabase(databaseName);

                    string sql = $"DROP TABLE {tableName}";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                MessageBox.Show($"테이블 {tableName}이(가) 삭제되었습니다.", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"테이블 삭제 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetCreateTableSQL(string tableName)
        {
            // 여기에 각 테이블의 생성 SQL을 정의합니다.
            // 예시로 Users 테이블의 SQL만 제공합니다.
            switch (tableName)
            {
                case "Users":
                    return @"
                        CREATE TABLE Users (
                            UserID INT PRIMARY KEY IDENTITY(1,1),
                            Username NVARCHAR(50) NOT NULL,
                            PasswordHash NVARCHAR(255) NOT NULL,
                            Email NVARCHAR(100) NOT NULL,
                            CreatedDate DATETIME NOT NULL,
                            LastLoginDate DATETIME
                        )";
                // 다른 테이블들의 SQL도 여기에 추가...
                default:
                    throw new NotImplementedException($"테이블 {tableName}의 생성 SQL이 정의되지 않았습니다.");
            }
        }
    }
}