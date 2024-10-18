using System;
using System.IO;

namespace PC1MAINAITradingSystem.Utils
{
    public class FileManager
    {
        public static string BasePath { get; private set; }
        public static string ERDPath { get; private set; }
        public static string ERDBackupPath { get; private set; }

        static FileManager()
        {
            BasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database");
            ERDPath = Path.Combine(BasePath, "PC1ERD.mermaid");
            ERDBackupPath = Path.Combine(BasePath, "ERD Backup");

            CreateDirectoryStructure();
        }

        private static void CreateDirectoryStructure()
        {
            Directory.CreateDirectory(BasePath);
            Directory.CreateDirectory(ERDBackupPath);
        }

        public static string SaveERD(string content, string fileName = null)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = "PC1ERD.mermaid";
            }
            string filePath = Path.Combine(BasePath, fileName);
            File.WriteAllText(filePath, content);
            return filePath;
        }

        public static string BackupERD(string sourceFilePath)
        {
            string fileName = $"PC1ERD_{DateTime.Now:yyyyMMdd_HHmmss}.mermaid";
            string destPath = Path.Combine(ERDBackupPath, fileName);
            File.Copy(sourceFilePath, destPath, true);
            return destPath;
        }
    }
}