```csharp
namespace PC1MAINAITradingSystem.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            // MenuStrip
            this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.erdMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.databaseMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.migrationMenu = new System.Windows.Forms.ToolStripMenuItem();

            // StatusStrip
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();

            // Main Container
            this.mainContainer = new System.Windows.Forms.SplitContainer();

            // ERD Viewer
            this.erdViewer = new System.Windows.Forms.Panel();

            // Database Explorer
            this.databaseExplorer = new System.Windows.Forms.TreeView();

            // Configure main form
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.MinimumSize = new System.Drawing.Size(800, 600);

            // Add controls
            this.Controls.Add(this.mainContainer);
            this.Controls.Add(this.mainMenuStrip);
            this.Controls.Add(this.statusStrip);

            this.MainMenuStrip = this.mainMenuStrip;
            this.Name = "MainForm";
            this.Text = "PC1 Main AI Trading System";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip mainMenuStrip;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.SplitContainer mainContainer;
        private System.Windows.Forms.Panel erdViewer;
        private System.Windows.Forms.TreeView databaseExplorer;
        private System.Windows.Forms.ToolStripMenuItem fileMenu;
        private System.Windows.Forms.ToolStripMenuItem erdMenu;
        private System.Windows.Forms.ToolStripMenuItem databaseMenu;
        private System.Windows.Forms.ToolStripMenuItem migrationMenu;
    }
}
```