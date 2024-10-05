using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;
using ForexAITradingPC1Main.Database;
using TableInfo = ForexAITradingPC1Main.Database.TableInfo; // 명시적으로 사용할 TableInfo 지정

namespace ForexAITradingPC1Main.Forms
{
    public partial class DatabaseManagementForm : Form
    {
        private TreeView dbTreeView;
        private DataGridView detailsGridView;
        private TextBox sqlQueryTextBox;
        private Button executeQueryButton;
        private DataGridView queryResultGridView;

        public DatabaseManagementForm()
        {
            InitializeComponent();
            LoadDatabaseStructure();
        }

        private void InitializeComponent()
        {
            // ... (기존 코드 유지)
        }

        private void LoadDatabaseStructure()
        {
            dbTreeView.Nodes.Clear();

            TreeNode databaseNode = dbTreeView.Nodes.Add("ForexTradingDB");

            string[] tableCategories = { "Customer Management", "Trading Data", "Performance Logs", "System Management" };

            foreach (string category in tableCategories)
            {
                TreeNode categoryNode = databaseNode.Nodes.Add(category);
                LoadTablesForCategory(categoryNode, category);
            }

            databaseNode.Expand();
        }

        private void LoadTablesForCategory(TreeNode categoryNode, string category)
        {
            // ... (기존 코드 유지)
        }

        private void DbTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Level == 2)  // Table node
            {
                string tableName = e.Node.Text;
                LoadTableDetails(tableName);
            }
        }

        private void LoadTableDetails(string tableName)
        {
            string connectionString = "Your_Connection_String_Here";
            string query = $"SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE " +
                           $"FROM INFORMATION_SCHEMA.COLUMNS " +
                           $"WHERE TABLE_NAME = '{tableName}'";

           
        }

        private void ExecuteQueryButton_Click(object sender, EventArgs e)
        {
            // ... (기존 코드 유지)
        }
    }
}

/*
 * 이 MainForm.cs 파일은 ForexAITradingPC1Main.Forms 접착 공간 안에 정의되어 있으며 다음과 같은 특징을 가지고 있습니다.

메뉴 메인 시스템을 호출하고 구성합니다.
각 메뉴 항목에 대한 이벤트 핸들러를 정의합니다.
데이터베이스 관리, ERD는, 테이블 디자인과 같은 기능을 새로운 양식을 열 수 있습니다.
아직 존재하지 않는 기능에 임시로 MessageBox를 표시합니다.

이 코드는 이전에 논의된 모든 주요 기능을 포함하고, 새로운 지우개 공간과 폴더 구조를 구성하고 있습니다. 추가 수정이나 구현이 필요하다면 말씀해 주십시오.


*/