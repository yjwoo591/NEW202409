using System;
using System.Windows.Forms;
using System.Drawing;

namespace PC1MAINAITradingSystem.Forms
{
    public class ImprovedERDViewerForm : Form
    {
        private RichTextBox erdRichTextBox;

        public ImprovedERDViewerForm(string erdContent)
        {
            InitializeComponent();
            DisplayERD(erdContent);
        }

        private void InitializeComponent()
        {
            this.Text = "ERD Viewer";
            this.Size = new Size(800, 600);

            erdRichTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10F),
                ReadOnly = true,
                WordWrap = false
            };

            this.Controls.Add(erdRichTextBox);
        }

        private void DisplayERD(string erdContent)
        {
            erdRichTextBox.Clear();
            string[] lines = erdContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (string line in lines)
            {
                if (line.Trim().StartsWith("erDiagram"))
                {
                    AppendText(line, Color.Blue, true);
                }
                else if (line.Contains("{") || line.Contains("}"))
                {
                    AppendText(line, Color.Green, true);
                }
                else if (line.Contains("--"))
                {
                    AppendText(line, Color.Red, false);
                }
                else
                {
                    AppendText(line, Color.Black, false);
                }
            }
        }

        private void AppendText(string text, Color color, bool bold)
        {
            erdRichTextBox.SelectionStart = erdRichTextBox.TextLength;
            erdRichTextBox.SelectionLength = 0;
            erdRichTextBox.SelectionColor = color;
            erdRichTextBox.SelectionFont = new Font(erdRichTextBox.Font, bold ? FontStyle.Bold : FontStyle.Regular);
            erdRichTextBox.AppendText(text + Environment.NewLine);
        }
    }
}