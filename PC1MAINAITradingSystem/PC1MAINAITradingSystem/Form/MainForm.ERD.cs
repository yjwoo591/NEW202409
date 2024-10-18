using System;
using System.Windows.Forms;
using System.IO;

namespace PC1MAINAITradingSystem.Forms
{
    public partial class MainForm
    {
        private void OnGenerateDatabaseFromERD(object sender, EventArgs e)
        {
            string erdContent = LoadERDContent();
            if (string.IsNullOrEmpty(erdContent))
            {
                MessageBox.Show("No ERD content loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var tables = _erdManager.ParseERD(erdContent);

            using (var structureForm = new DatabaseStructureForm(tables))
            {
                if (structureForm.ShowDialog() == DialogResult.OK)
                {
                    var updatedTables = structureForm.GetUpdatedTables();
                    var updatedERD = _erdManager.ConvertTablesToERD(updatedTables);
                    var scripts = _erdManager.GenerateDatabaseScripts(updatedERD);

                    if (MessageBox.Show("Do you want to apply these database scripts?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        _erdManager.ApplyDatabaseScripts(scripts, _dbManager);
                        MessageBox.Show("Database generated successfully from ERD.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    AddLog("Database structure reviewed and updated");
                }
            }
        }

        private void OnReadERD(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "ERD Files (*.mermaid)|*.mermaid|All Files (*.*)|*.*";
                openFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, "Database");

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string erdContent = _erdManager.ReadERD(openFileDialog.FileName);
                    AddLog($"ERD file read: {openFileDialog.FileName}");
                }
            }
        }

        private void OnSaveERD(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "ERD Files (*.mermaid)|*.mermaid|All Files (*.*)|*.*";
                saveFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, "Database");
                saveFileDialog.FileName = "PC1ERD.mermaid";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string erdContent = "... ERD content ..."; // 실제 ERD 내용으로 대체해야 함
                    _erdManager.SaveERD(erdContent, saveFileDialog.FileName);
                    AddLog($"ERD file saved: {saveFileDialog.FileName}");
                }
            }
        }

        private void OnBackupERD(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "ERD Files (*.mermaid)|*.mermaid|All Files (*.*)|*.*";
                openFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, "Database");

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _erdManager.BackupERD(openFileDialog.FileName);
                    AddLog($"ERD file backed up: {openFileDialog.FileName}");
                }
            }
        }

        private string LoadERDContent()
        {
            string erdPath = Path.Combine(
                Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                "..", "..", "Database", "PC1ERD.mermaid");

            if (File.Exists(erdPath))
            {
                return File.ReadAllText(erdPath);
            }
            else
            {
                MessageBox.Show("ERD file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }
        }
    }
}