using System;
using System.Drawing;
using System.Windows.Forms;

namespace PC1MAINAITradingSystem.Forms
{
    public partial class MainForm
    {
        partial void InitializeUI()
        {
            CreateStatusBar();
            CreateSplitContainer();
            CreateLogWindow();

            this.FormClosing += MainForm_FormClosing;
        }

        private void CreateStatusBar()
        {
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            statusLabel.Text = "Ready";
            statusStrip.Items.Add(statusLabel);
            this.Controls.Add(statusStrip);
        }

        private void CreateSplitContainer()
        {
            splitContainer = new SplitContainer();
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Orientation = Orientation.Vertical;
            splitContainer.SplitterDistance = this.Width - 300;

            mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;

            splitContainer.Panel1.Controls.Add(mainPanel);
            this.Controls.Add(splitContainer);
        }

        private void CreateLogWindow()
        {
            logTextBox = new TextBox();
            logTextBox.Multiline = true;
            logTextBox.ReadOnly = true;
            logTextBox.Dock = DockStyle.Fill;
            logTextBox.Font = new Font("Consolas", 9F);
            logTextBox.BackColor = Color.Black;
            logTextBox.ForeColor = Color.Lime;

            splitContainer.Panel2.Controls.Add(logTextBox);
        }

        // MainForm_FormClosing 이벤트 핸들러는 이미 MainForm.cs에 정의되어 있으므로 여기서는 제거합니다.
    }
}