using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PC1MAINAITradingSystem.Forms
{
    public class ValidationForm : Form
    {
        private readonly ListView _resultListView;
        private readonly Button _closeButton;
        private readonly Button _exportButton;

        public ValidationForm()
        {
            Text = "ERD 검증 결과";
            Size = new Size(600, 400);
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;

            _resultListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            _resultListView.Columns.AddRange(new[]
            {
                new ColumnHeader { Text = "유형", Width = 100 },
                new ColumnHeader { Text = "위치", Width = 150 },
                new ColumnHeader { Text = "설명", Width = 300 }
            });

            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50
            };

            _exportButton = new Button
            {
                Text = "내보내기",
                Width = 100,
                Height = 30,
                Location = new Point(buttonPanel.Width - 230, 10)
            };
            _exportButton.Click += ExportButton_Click;

            _closeButton = new Button
            {
                Text = "닫기",
                Width = 100,
                Height = 30,
                Location = new Point(buttonPanel.Width - 120, 10)
            };
            _closeButton.Click += (s, e) => Close();

            buttonPanel.Controls.AddRange(new Control[] { _exportButton, _closeButton });

            Controls.AddRange(new Control[] { _resultListView, buttonPanel });
        }

        public void AddValidationResult(string type, string location, string description)
        {
            var item = new ListViewItem(new[] { type, location, description });

            switch (type.ToLower())
            {
                case "error":
                    item.ForeColor = Color.Red;
                    break;
                case "warning":
                    item.ForeColor = Color.Orange;
                    break;
                case "info":
                    item.ForeColor = Color.Blue;
                    break;
            }

            _resultListView.Items.Add(item);
        }

        private async void ExportButton_Click(object sender, EventArgs e)
        {
            using var saveDialog = new SaveFileDialog
            {
                Filter = "CSV 파일 (*.csv)|*.csv|모든 파일 (*.*)|*.*",
                FilterIndex = 1,
                DefaultExt = "csv"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using var writer = new System.IO.StreamWriter(saveDialog.FileName);
                    await writer.WriteLineAsync("유형,위치,설명");

                    foreach (ListViewItem item in _resultListView.Items)
                    {
                        var line = string.Join(",", new[]
                        {
                            EscapeCsvField(item.SubItems[0].Text),
                            EscapeCsvField(item.SubItems[1].Text),
                            EscapeCsvField(item.SubItems[2].Text)
                        });
                        await writer.WriteLineAsync(line);
                    }

                    MessageBox.Show("검증 결과가 성공적으로 저장되었습니다.",
                                  "저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"파일 저장 중 오류가 발생했습니다.\n{ex.Message}",
                                  "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";

            if (field.Contains("\""))
                field = field.Replace("\"", "\"\"");

            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
                field = $"\"{field}\"";

            return field;
        }
    }
}