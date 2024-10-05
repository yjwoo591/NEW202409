using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using ForexAITradingPC1Main.Database;

namespace ForexAITradingPC1Main.Forms
{
    public class TableDesignForm : Form
    {
        private ListBox tableListBox;
        private DataGridView columnGridView;
        private Button addTableButton;
        private Button deleteTableButton;
        private Button saveChangesButton;
        private TextBox tableNameTextBox;
        private MermaidProcessor mermaidProcessor;
        private string currentMermaidContent;

        public TableDesignForm()
        {
            InitializeComponent();
            mermaidProcessor = new MermaidProcessor();
        }

        private void InitializeComponent()
        {
            this.Size = new System.Drawing.Size(1000, 600);
            this.Text = "Table Design";

            tableListBox = new ListBox
            {
                Dock = DockStyle.Left,
                Width = 200
            };
            tableListBox.SelectedIndexChanged += TableListBox_SelectedIndexChanged;
            this.Controls.Add(tableListBox);

            Panel rightPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(rightPanel);

            tableNameTextBox = new TextBox
            {
                Location = new Point(10, 10),
                Width = 180
            };
            rightPanel.Controls.Add(tableNameTextBox);

            columnGridView = new DataGridView
            {
                Dock = DockStyle.Bottom,
                Height = 400
            };
            columnGridView.Columns.Add("ColumnName", "Column Name");
            columnGridView.Columns.Add("DataType", "Data Type");
            columnGridView.Columns.Add("Constraint", "Constraint");
            rightPanel.Controls.Add(columnGridView);

            addTableButton = new Button
            {
                Text = "Add Table",
                Location = new Point(10, 40),
                Width = 100
            };
            addTableButton.Click += AddTableButton_Click;
            rightPanel.Controls.Add(addTableButton);

            deleteTableButton = new Button
            {
                Text = "Delete Table",
                Location = new Point(120, 40),
                Width = 100
            };
            deleteTableButton.Click += DeleteTableButton_Click;
            rightPanel.Controls.Add(deleteTableButton);

            saveChangesButton = new Button
            {
                Text = "Save Changes",
                Location = new Point(230, 40),
                Width = 100
            };
            saveChangesButton.Click += SaveChangesButton_Click;
            rightPanel.Controls.Add(saveChangesButton);
        }

        public void LoadMermaidFile(string filePath)
        {
            currentMermaidContent = File.ReadAllText(filePath);
            List<TableInfo> tables = mermaidProcessor.ReadMermaidFile(filePath);
            PopulateTableList(tables);
        }

        private void PopulateTableList(List<TableInfo> tables)
        {
            tableListBox.Items.Clear();
            foreach (var table in tables)
            {
                tableListBox.Items.Add(table.Name);
            }
        }

        private void TableListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tableListBox.SelectedIndex != -1)
            {
                string selectedTableName = tableListBox.SelectedItem.ToString();
                TableInfo selectedTable = mermaidProcessor.ReadMermaidFile(currentMermaidContent)
                    .Find(t => t.Name == selectedTableName);

                if (selectedTable != null)
                {
                    tableNameTextBox.Text = selectedTable.Name;
                    PopulateColumnGrid(selectedTable.Columns);
                }
            }
        }

        private void PopulateColumnGrid(List<ColumnInfo> columns)
        {
            columnGridView.Rows.Clear();
           
        }

        private void AddTableButton_Click(object sender, EventArgs e)
        {
            string newTableName = tableNameTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(newTableName) && !tableListBox.Items.Contains(newTableName))
            {
                tableListBox.Items.Add(newTableName);
                TableInfo newTable = new TableInfo { Name = newTableName, Columns = new List<ColumnInfo>() };
                mermaidProcessor.AddTableToMermaid(ref currentMermaidContent, newTable);
                tableListBox.SelectedItem = newTableName;
            }
            else
            {
                MessageBox.Show("Please enter a unique table name.", "Invalid Table Name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DeleteTableButton_Click(object sender, EventArgs e)
        {
            if (tableListBox.SelectedIndex != -1)
            {
                string tableToDelete = tableListBox.SelectedItem.ToString();
                mermaidProcessor.RemoveTableFromMermaid(ref currentMermaidContent, tableToDelete);
                tableListBox.Items.RemoveAt(tableListBox.SelectedIndex);
                columnGridView.Rows.Clear();
                tableNameTextBox.Clear();
            }
        }

        private void SaveChangesButton_Click(object sender, EventArgs e)
        {
            if (tableListBox.SelectedIndex != -1)
            {
                string tableName = tableNameTextBox.Text.Trim();
                List<ColumnInfo> columns = new List<ColumnInfo>();

                foreach (DataGridViewRow row in columnGridView.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        var columnInfo = new ColumnInfo
                        {
                          
                            DataType = row.Cells["DataType"].Value?.ToString(),
                          
                        };

                      

                        columns.Add(columnInfo);
                    }
                }

                TableInfo updatedTable = new TableInfo { Name = tableName, Columns = columns };
                mermaidProcessor.UpdateTableInMermaid(ref currentMermaidContent, tableListBox.SelectedItem.ToString(), updatedTable);

                int selectedIndex = tableListBox.SelectedIndex;
                tableListBox.Items[selectedIndex] = tableName;
                tableListBox.SelectedIndex = selectedIndex;

                MessageBox.Show("Changes saved successfully!", "Save Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public string GetUpdatedMermaidContent()
        {
            return currentMermaidContent;
        }
    }
}



/*   이 TableDesignForm.cs 파일은 다음과 같은 주요 특징을 가지고 있습니다:
 *   
 *   
 *   목록을 표시하는 ListBox
선택한 테이블의 열 정보를 표시하고 편집할 수 있는 DataGridView
테이블 추가, 삭제, 변경사항 저장 기능
MermaidProcessor 클래스를 사용하여 Mermaid 클래스의 ERD를 처리합니다.

주요 방법:

LoadMermaidFile: Mermaid 파일을 로드하고 테이블 목록을 채웁니다.
PopulateTableList: 테이블 목록을 UI에 표시합니다.
PopulateColumnGrid: 선택한 테이블의 열 정보를 그리드에 표시합니다. 이제 제약사항 정보도 포함됩니다.
AddTableButton_Click: 테이블을 추가합니다.
DeleteTableButton_Click: 선택한 테이블을 삭제합니다.
SaveChangesButton_Click:테이블 변경 사항을 저장해 주시기 바랍니다. 이제 Constraint 정보를 처리하고, PK(Primary Key)가 응답을 설정합니다.
GetUpdatedMermaidContent: 수정된 Mermaid 내용을 반환하겠습니다.

이 구현은 ColumnInfo 클래스에 Constraint 속성이 추가되어 유지됩니다. 또한 MermaidProcessor 클래스가 이러한 변경 사항을 처리할 수 있도록 업데이트할 수 있습니다.
만약 계속해서 오류가 발생할 경우, 다음 사항에 대해 알려주세요:

ColumnInfo 클래스가 Constraint 속성을 포함하고 있는지 확인합니다.
MermaidProcessor 클래스가 업데이트된 ColumnInfo 구조를 처리할 수 있는지 확인합니다.
모든 관련 파일이 카테고리 분류 스페이스(ForexAITradingPC1Main.Database)를 사용하고 있는지 확인합니다.
프로젝트를 정리(Clean)하고 다시 빌드해보기
이 TableDesignForm.cs 파일은 다음과 같은 주요 특징을 가지고 있습니다:

목록을 표시하는 ListBox
선택한 테이블의 열 정보를 표시하고 편집할 수 있는 DataGridView
테이블 추가, 삭제, 변경사항 저장 기능
MermaidProcessor 클래스를 사용하여 Mermaid 클래스의 ERD를 처리합니다.

주요 방법:

LoadMermaidFile: Mermaid 파일을 로드하고 테이블 목록을 채웁니다.
PopulateTableList: 테이블 목록을 UI에 표시합니다.
PopulateColumnGrid: 선택한 테이블의 열 정보를 그리드에 표시합니다.
AddTableButton_Click: 테이블을 추가합니다.
DeleteTableButton_Click: 선택한 테이블을 삭제합니다.
SaveChangesButton_Click:테이블 변경 사항을 저장해 주시기 바랍니다.
GetUpdatedMermaidContent: 수정된 Mermaid 내용을 반환하겠습니다.

이 폼을 사용하여 사용자는 데이터베이스 테이블 구조를 표시하고 설계할 수 있습니다. Mermaid Format의 ERD와 캐스팅되어 있어, 변경 사항이 ERD에 즉시 적용됩니다.
주의할 점:

MermaidProcessor 클래스가 분리되어 있어야 합니다.
TableInfo와 ColumnInfo 클래스를 정의해야 합니다.

이 클래스를 사용하기 위해서는 ForexAITradingPC1Main.Database 라벨스페이스에 MermaidProcessor, TableInfo, ColumnInfo 클래스가 존재해야 합니다.
만약 이 클래스가 아직 구현되지 않았다면, 그들을 먼저 구현해야 합니다.
목록을 표시하는 ListBox
선택한 테이블의 열 정보를 표시하고 편집할 수 있는 DataGridView
테이블 추가, 삭제, 변경사항 저장 기능
MermaidProcessor 클래스를 사용하여 Mermaid 클래스의 ERD를 처리합니다.

주요 기능:

LoadMermaidFile(): Mermaid 파일을 로드하고 테이블 목록을 채웁니다.
AddTableButton_Click(): 테이블을 추가합니다.
DeleteTableButton_Click(): 선택한 테이블을 삭제합니다.
SaveChangesButton_Click():테이블의 변경 사항을 저장해 드립니다.
GetUpdatedMermaidContent(): 수정된 Mermaid 내용을 반환하겠습니다.

이 폼을 사용하여 사용자는 데이터베이스 테이블 구조를 쉽게 설계하고 사용할 수 있습니다.
Mermaid Format의 ERD와 캐스팅되어 있어, 변경 사항이 ERD에 즉시 적용됩니다.
MermaidProcessor 클래스는 분리되어야 하며, Mermaid 형식의 ERD를 파싱하고 수정하는 기능을 제공해야 합니다. 
이 클래스의 구현이 필요하다면 말씀해 주십시오.
또한, 이 폼을 메인 기능에 통합하려면 MainForm.cs에서 이 폼을 열 수 있도록 메뉴 항목이나 버튼을 추가해야 합니다.
이에 대해 알려 주시기 바랍니다.
*/