using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using ForexAITradingSystem.PopupForms;
using ForexAITradingSystem.Utilities;

namespace ForexAITradingSystem.PopupForms
{
    public partial class DatabaseManagerForm : Form
    {
        private ListBox listDatabases;
        private Button btnCreate;
        private Button btnDelete;
        private Button btnShowERD;
        private Button btnAddNew;  // 이 줄을 추가합니다.
        private string connectionString;

        private List<string> suggestedDatabases;
        private List<string> deletedDatabases;

        public DatabaseManagerForm(string connectionString)
        {
            this.connectionString = connectionString;
            suggestedDatabases = new List<string>
            {
                "ForexAI_UserManagement",
                "ForexAI_MarketData",
                "ForexAI_TradingManagement",
                "ForexAI_AIModels",
                "ForexAI_SystemLogs"
            };
            deletedDatabases = new List<string>();
            InitializeComponent();
            LoadDatabases();
        }

        private void InitializeComponent()
        {
            this.listDatabases = new ListBox();
            this.btnCreate = new Button();
            this.btnDelete = new Button();
            this.btnShowERD = new Button();
            this.btnAddNew = new Button();  // 이 줄을 추가합니다.

            // listDatabases
            this.listDatabases.Dock = DockStyle.Left;
            this.listDatabases.SelectionMode = SelectionMode.MultiExtended;
            this.listDatabases.Width = 250;

            // btnCreate
            this.btnCreate.Text = "DB 생성";
            this.btnCreate.Dock = DockStyle.Bottom;
            this.btnCreate.Click += BtnCreate_Click;

            // btnDelete
            this.btnDelete.Text = "DB 삭제";
            this.btnDelete.Dock = DockStyle.Bottom;
            this.btnDelete.Click += BtnDelete_Click;

            // btnShowERD
            this.btnShowERD.Text = "Show ERD";
            this.btnShowERD.Dock = DockStyle.Bottom;
            this.btnShowERD.Click += BtnShowERD_Click;

            // btnAddNew
            this.btnAddNew.Text = "새 DB 추가";
            this.btnAddNew.Dock = DockStyle.Bottom;
            this.btnAddNew.Click += BtnAddNew_Click;

            // DatabaseManagerForm
            this.ClientSize = new System.Drawing.Size(400, 330);
            this.Controls.Add(this.listDatabases);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnShowERD);
            this.Controls.Add(this.btnAddNew);
            this.Text = "Database Manager";

            this.listDatabases.DoubleClick += new EventHandler(this.listDatabases_DoubleClick);
        }

        private void LoadDatabases()
        {
            listDatabases.Items.Clear();
            List<string> existingDatabases = new List<string>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("SELECT name FROM sys.databases WHERE database_id > 4", connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        existingDatabases.Add(reader["name"].ToString());
                    }
                }
            }

            foreach (string db in existingDatabases.Concat(suggestedDatabases).Concat(deletedDatabases).Distinct())
            {
                if (existingDatabases.Contains(db))
                {
                    listDatabases.Items.Add(db);
                }
                else
                {
                    listDatabases.Items.Add($"[생성 가능] {db}");
                }
            }

            listDatabases.Items.Add("< 새 데이터베이스 추가 >");
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            List<string> databasesToCreate = new List<string>();

            if (listDatabases.SelectedItems.Count == 0 || listDatabases.SelectedItem.ToString() == "< 새 데이터베이스 추가 >")
            {
                string newDbName = Microsoft.VisualBasic.Interaction.InputBox("새 데이터베이스 이름을 입력하세요:", "새 데이터베이스", "");
                if (!string.IsNullOrWhiteSpace(newDbName))
                {
                    databasesToCreate.Add(newDbName);
                }
            }
            else
            {
                foreach (object item in listDatabases.SelectedItems)
                {
                    string dbName = item.ToString();
                    if (dbName.StartsWith("[생성 가능] "))
                    {
                        databasesToCreate.Add(dbName.Substring(11));
                    }
                }
            }

            foreach (string dbName in databasesToCreate)
            {
                CreateDatabase(dbName);
                deletedDatabases.Remove(dbName);
            }

            LoadDatabases();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("선택한 데이터베이스를 삭제하시겠습니까?", "확인", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                foreach (object item in listDatabases.SelectedItems)
                {
                    string dbName = item.ToString();
                    if (dbName != "< 새 데이터베이스 추가 >" && !dbName.StartsWith("[생성 가능] "))
                    {
                        DeleteDatabase(dbName);
                        if (!suggestedDatabases.Contains(dbName))
                        {
                            deletedDatabases.Add(dbName);
                        }
                    }
                }
                LoadDatabases();
            }
        }

        private void BtnAddNew_Click(object sender, EventArgs e)
        {
            string newDbName = Microsoft.VisualBasic.Interaction.InputBox("새 데이터베이스 이름을 입력하세요:", "새 데이터베이스", "");
            if (!string.IsNullOrWhiteSpace(newDbName))
            {
                CreateDatabase(newDbName);
                LoadDatabases();
            }
        }
        private void CreateDatabase(string dbName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = $"CREATE DATABASE {dbName}";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                MessageBox.Show($"데이터베이스 {dbName}가 생성되었습니다.", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터베이스 생성 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteDatabase(string dbName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = $"DROP DATABASE {dbName}";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                MessageBox.Show($"데이터베이스 {dbName}가 삭제되었습니다.", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터베이스 삭제 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void listDatabases_DoubleClick(object sender, EventArgs e)
        {
            if (listDatabases.SelectedItem != null)
            {
                string selectedDb = listDatabases.SelectedItem.ToString();
                if (!selectedDb.StartsWith("[생성 가능]") && selectedDb != "< 새 데이터베이스 추가 >")
                {
                    using (var tableManagerForm = new TableManagerForm(connectionString, selectedDb))
                    {
                        tableManagerForm.ShowDialog();
                    }
                }
            }
        }

        private void BtnShowERD_Click(object sender, EventArgs e)
        {
            using (var erdForm = new ERDVisualizerForm())
            {
                erdForm.ShowDialog();
            }
        }
    }
}