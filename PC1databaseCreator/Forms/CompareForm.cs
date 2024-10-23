using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using PC1MAINAITradingSystem.Core.Comparator;
using PC1MAINAITradingSystem.Core.ERD.Comparator;
using PC1MAINAITradingSystem.Core.ERD.Parser;

namespace PC1MAINAITradingSystem.Forms
{
    public class CompareForm : Form
    {
        private readonly TextBox _oldErdTextBox;
        private readonly TextBox _newErdTextBox;
        private readonly RichTextBox _differenceTextBox;
        private readonly Button _compareButton;
        private readonly Button _exportButton;
        private readonly Button _closeButton;

        public CompareForm()
        {
            Text = "ERD 비교";
            Size = new Size(800, 600);
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 2,
                Padding = new Padding(10)
            };

            // 이전 ERD 섹션
            mainPanel.Controls.Add(new Label { Text = "이전 ERD:" }, 0, 0);
            _oldErdTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true
            };
            var oldErdPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 0, 0, 5)
            };
            var oldBrowseButton = new Button
            {
                Text = "찾아보기",
                Width = 80
            };
            oldBrowseButton.Click += async (s, e) => await BrowseOldERD();
            oldErdPanel.Controls.AddRange(new Control[] { _oldErdTextBox, oldBrowseButton });
            mainPanel.Controls.Add(oldErdPanel, 1, 0);

            // 새 ERD 섹션
            mainPanel.Controls.Add(new Label { Text = "새 ERD:" }, 0, 1);
            _newErdTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true
            };
            var newErdPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 0, 0, 5)
            };
            var newBrowseButton = new Button
            {
                Text = "찾아보기",
                Width = 80
            };
            newBrowseButton.Click += async (s, e) => await BrowseNewERD();
            newErdPanel.Controls.AddRange(new Control[] { _newErdTextBox, newBrowseButton });
            mainPanel.Controls.Add(newErdPanel, 1, 1);

            // 비교 결과 섹션
            mainPanel.Controls.Add(new Label { Text = "차이점:" }, 0, 2);
            _differenceTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 9F)
            };
            mainPanel.Controls.Add(_differenceTextBox, 1, 2);

            // 버튼 패널
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 40,
                Padding = new Padding(10, 5, 10, 5)
            };

            _closeButton = new Button
            {
                Text = "닫기",
                Width = 80
            };
            _closeButton.Click += (s, e) => Close();

            _exportButton = new Button
            {
                Text = "내보내기",
                Width = 80,
                Enabled = false
            };
            _exportButton.Click += async (s, e) => await ExportComparison();

            _compareButton = new Button
            {
                Text = "비교",
                Width = 80
            };
            _compareButton.Click += async (s, e) => await CompareERDs();

            buttonPanel.Controls.AddRange(new Control[] { _closeButton, _exportButton, _compareButton });

            Controls.AddRange(new Control[] { mainPanel, buttonPanel });

            // 레이아웃 설정
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        }

        private async Task BrowseOldERD()
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "ERD 파일 (*.mermaid)|*.mermaid|모든 파일 (*.*)|*.*",
                FilterIndex = 1
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _oldErdTextBox.Text = dialog.FileName;
                await UpdateCompareButtonState();
            }
        }

        private async Task BrowseNewERD()
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "ERD 파일 (*.mermaid)|*.mermaid|모든 파일 (*.*)|*.*",
                FilterIndex = 1
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _newErdTextBox.Text = dialog.FileName;
                await UpdateCompareButtonState();
            }
        }

        private async Task UpdateCompareButtonState()
        {
            _compareButton.Enabled = !string.IsNullOrEmpty(_oldErdTextBox.Text) &&
                                   !string.IsNullOrEmpty(_newErdTextBox.Text);
        }

        private async Task CompareERDs()
        {
            try
            {
                _differenceTextBox.Clear();
                var parser = new ERDParser();
                var comparer = new ERDComparer();

                // 파일 로드 및 파싱
                var oldContent = await System.IO.File.ReadAllTextAsync(_oldErdTextBox.Text);
                var newContent = await System.IO.File.ReadAllTextAsync(_newErdTextBox.Text);

                var oldModel = await parser.ParseMermaid(oldContent);
                var newModel = await parser.ParseMermaid(newContent);

                // 비교 수행
                var result = await comparer.CompareERD(oldModel, newModel);
                var report = await comparer.GenerateChangeReport(result);

                _differenceTextBox.Text = report;
                _exportButton.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ERD 비교 중 오류가 발생했습니다.\n{ex.Message}",
                              "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ExportComparison()
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "텍스트 파일 (*.txt)|*.txt|모든 파일 (*.*)|*.*",
                FilterIndex = 1,
                DefaultExt = "txt"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    await System.IO.File.WriteAllTextAsync(dialog.FileName, _differenceTextBox.Text);
                    MessageBox.Show("비교 결과가 성공적으로 저장되었습니다.",
                                  "저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"파일 저장 중 오류가 발생했습니다.\n{ex.Message}",
                                  "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}