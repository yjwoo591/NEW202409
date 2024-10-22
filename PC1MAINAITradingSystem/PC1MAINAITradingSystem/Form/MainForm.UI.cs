using System;
using System.Windows.Forms;

namespace PC1MAINAITradingSystem.Forms
{
    public partial class MainForm
    {
        private Panel mainPanel;
        private TextBox logTextBox;

        partial void InitializeUI()
        {
            // Initialize mainPanel
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(mainPanel);

            // Initialize statusStrip and statusLabel
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            statusStrip.Items.Add(statusLabel);
            this.Controls.Add(statusStrip);

            // Initialize splitContainer
            splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical
            };
            mainPanel.Controls.Add(splitContainer);

            // Initialize logTextBox
            logTextBox = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                ReadOnly = true
            };
            splitContainer.Panel2.Controls.Add(logTextBox);

            // You can add more UI components here as needed
        }
    }
}