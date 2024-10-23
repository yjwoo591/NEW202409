using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using PC1MAINAITradingSystem.Core.Parser;
using PC1MAINAITradingSystem.Core.Generator;
using PC1MAINAITradingSystem.Core.Comparator;
using PC1MAINAITradingSystem.Core.ERD.Comparator;
using PC1MAINAITradingSystem.Core.ERD.Generator;
using PC1MAINAITradingSystem.Core.ERD.Parser;

namespace PC1MAINAITradingSystem.Forms
{
    public partial class MainForm
    {
        private ERDParser _erdParser;
        private ERDGenerator _erdGenerator;
        private ERDComparer _erdComparer;
        private ERDModel _currentERD;
        private string _currentERDPath;
        private bool _isERDModified;

        private readonly TabControl _mainTabControl;
        private readonly RichTextBox _erdTextBox;
        private readonly Panel _erdVisualPanel;

        partial void InitializeERDComponents()
        {
            _erdParser = new ERDParser();
            _erdGenerator = new ERDGenerator();
            _erdComparer = new ERDComparer();

            _mainTabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // Text 탭
            var textTab = new TabPage("Text View");
            _erdTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10F),
                AcceptsTab = true,
                WordWrap = false
            };
            _erdTextBox.TextChanged += ERDTextBox_TextChanged;
            textTab.Controls.Add(_erdTextBox);

            // Visual 탭
            var visualTab = new TabPage("Visual View");
            _erdVisualPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
            visualTab.Controls.Add(_erdVisualPanel);

            _mainTabControl.TabPages.AddRange(new[] { textTab, visualTab });
            Controls.Add(_mainTabControl);
        }

        private async void ERDTextBox_TextChanged(object sender, EventArgs e)
        {
            _isERDModified = true;
            await UpdateVisualView();
        }

        private async Task LoadERDFile(string filePath)
        {
            try
            {
                string content = await File.ReadAllTextAsync(filePath);

                // ERD 파싱 및 검증
                if (await _erdParser.ValidateSyntax(content))
                {
                    _currentERD = await _erdParser.ParseMermaid(content);
                    _currentERDPath = filePath;
                    _erdTextBox.Text = content;
                    _isERDModified = false;

                    await _logger.Info($"ERD file loaded: {filePath}");
                }
            }
            catch (Exception ex)
            {
                await _logger.Error($"Failed to load ERD file: {filePath}", ex);
                MessageBox.Show("ERD 파일을 로드하는 중 오류가 발생했습니다.",
                              "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task SaveERDFile(string filePath)
        {
            try
            {
                await File.WriteAllTextAsync(filePath, _erdTextBox.Text);
                _currentERDPath = filePath;
                _isERDModified = false;

                await _logger.Info($"ERD file saved: {filePath}");
            }
            catch (Exception ex)
            {
                await _logger.Error($"Failed to save ERD file: {filePath}", ex);
                MessageBox.Show("ERD 파일을 저장하는 중 오류가 발생했습니다.",
                              "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task UpdateVisualView()
        {
            try
            {
                if (_mainTabControl.SelectedTab.Text == "Visual View")
                {
                    string content = _erdTextBox.Text;
                    if (await _erdParser.ValidateSyntax(content))
                    {
                        _currentERD = await _erdParser.ParseMermaid(content);
                        await RenderERDVisual(_currentERD);
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.Error("Failed to update visual view", ex);
            }
        }

        private async Task RenderERDVisual(ERDModel model)
        {
            try
            {
                _erdVisualPanel.SuspendLayout();
                _erdVisualPanel.Controls.Clear();

                // ERD 시각화 구현
                const int TABLE_WIDTH = 200;
                const int TABLE_MARGIN = 20;
                int x = TABLE_MARGIN;
                int y = TABLE_MARGIN;
                int maxHeight = 0;

                foreach (var table in model.Tables)
                {
                    var tablePanel = CreateTablePanel(table);
                    tablePanel.Location = new Point(x, y);
                    _erdVisualPanel.Controls.Add(tablePanel);

                    x += TABLE_WIDTH + TABLE_MARGIN;
                    maxHeight = Math.Max(maxHeight, tablePanel.Height);

                    if (x + TABLE_WIDTH > _erdVisualPanel.Width)
                    {
                        x = TABLE_MARGIN;
                        y += maxHeight + TABLE_MARGIN;
                        maxHeight = 0;
                    }
                }

                _erdVisualPanel.ResumeLayout();
            }
            catch (Exception ex)
            {
                await _logger.Error("Failed to render ERD visual", ex);
            }
        }

        private Panel CreateTablePanel(TableModel table)
        {
            var panel = new Panel
            {
                Width = 200,
                BorderStyle = BorderStyle.FixedSingle
            };

            var titleLabel = new Label
            {
                Text = table.Name,
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(Font, FontStyle.Bold),
                BackColor = SystemColors.ActiveCaption
            };
            panel.Controls.Add(titleLabel);

            var columnPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                Padding = new Padding(5)
            };

            int y = 0;
            foreach (var column in table.Columns)
            {
                var columnLabel = new Label
                {
                    Text = $"{column.Name}: {column.Type}",
                    AutoSize = true,
                    Location = new Point(5, y)
                };

                if (column.IsPrimaryKey)
                {
                    columnLabel.Font = new Font(columnLabel.Font, FontStyle.Bold);
                    columnLabel.Text = "🔑 " + columnLabel.Text;
                }
                else if (column.IsForeignKey)
                {
                    columnLabel.Font = new Font(columnLabel.Font, FontStyle.Italic);
                    columnLabel.Text = "🔗 " + columnLabel.Text;
                }

                columnPanel.Controls.Add(columnLabel);
                y += columnLabel.Height + 2;
            }

            panel.Controls.Add(columnPanel);
            panel.Height = titleLabel.Height + columnPanel.Height;

            return panel;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_isERDModified)
            {
                var result = MessageBox.Show(
                    "변경된 내용을 저장하시겠습니까?",
                    "저장",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                switch (result)
                {
                    case DialogResult.Yes:
                        SaveERD().Wait();
                        break;
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }

            base.OnFormClosing(e);
        }
    }
}