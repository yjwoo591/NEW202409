using System;
using System.Windows.Forms;

namespace PC1MAINAITradingSystem.Forms
{
    public class ERDViewerForm : Form
    {
        private TextBox erdTextBox;

        public ERDViewerForm(string erdContent)
        {
            InitializeComponent();
            erdTextBox.Text = erdContent;
        }

        private void InitializeComponent()
        {
            this.Text = "ERD Viewer";
            this.Size = new System.Drawing.Size(800, 600);

            erdTextBox = new TextBox();
            erdTextBox.Multiline = true;
            erdTextBox.ScrollBars = ScrollBars.Both;
            erdTextBox.Dock = DockStyle.Fill;
            erdTextBox.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            erdTextBox.ReadOnly = true;

            this.Controls.Add(erdTextBox);
        }
    }
}