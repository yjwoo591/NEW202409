using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using PC1MAINAITradingSystem.Database;

namespace PC1MAINAITradingSystem.Database
{
    public class ERDManager
    {
        private const string BACKUP_FOLDER = "ERD Backup";

        public string ReadERD(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("ERD file not found.", filePath);
            }

            return File.ReadAllText(filePath);
        }

        public void SaveERD(string content, string filePath)
        {
            File.WriteAllText(filePath, content);
        }


        public string ConvertTablesToERD(List<Table> tables)
        {
            var erdBuilder = new System.Text.StringBuilder();
            erdBuilder.AppendLine("erDiagram");
            foreach (var table in tables)
            {
                erdBuilder.AppendLine($"    {table.Name} {{");
                foreach (var column in table.Columns)
                {
                    erdBuilder.AppendLine($"        {column.Type} {column.Name} {column.Constraint}");
                }
                erdBuilder.AppendLine("    }");
            }
            return erdBuilder.ToString();
        }

        public void BackupERD(string sourceFilePath)
        {
            if (!File.Exists(sourceFilePath))
            {
                throw new FileNotFoundException("ERD file not found for backup.", sourceFilePath);
            }

            string fileName = Path.GetFileName(sourceFilePath);
            string backupFolder = Path.Combine(Path.GetDirectoryName(sourceFilePath), BACKUP_FOLDER);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{timestamp}{Path.GetExtension(fileName)}";
            string backupFilePath = Path.Combine(backupFolder, backupFileName);

            Directory.CreateDirectory(backupFolder);
            File.Copy(sourceFilePath, backupFilePath, true);
        }

        public List<DatabaseScript> GenerateDatabaseScripts(string erdContent)
        {
            var scripts = new List<DatabaseScript>();
            var tables = ParseERD(erdContent);

            foreach (var table in tables)
            {
                scripts.Add(new DatabaseScript
                {
                    DatabaseName = "MainDatabase", // 실제 구현에서는 적절한 데이터베이스 이름을 사용해야 합니다.
                    Script = GenerateCreateTableScript(table)
                });
            }

            return scripts;
        }

        private List<Table> ParseERD(string erdContent)
        {
            var tables = new List<Table>();
            var tableRegex = new Regex(@"(\w+)\s*{([^}]+)}");
            var columnRegex = new Regex(@"(\w+)\s+(\w+(?:\(\d+\))?)(?:\s+(\w+))?");

            foreach (Match tableMatch in tableRegex.Matches(erdContent))
            {
                var tableName = tableMatch.Groups[1].Value;
                var tableContent = tableMatch.Groups[2].Value;
                var table = new Table { Name = tableName };

                foreach (Match columnMatch in columnRegex.Matches(tableContent))
                {
                    table.Columns.Add(new Column
                    {
                        Name = columnMatch.Groups[1].Value,
                        Type = columnMatch.Groups[2].Value,
                        Constraint = columnMatch.Groups[3].Success ? columnMatch.Groups[3].Value : null
                    });
                }

                tables.Add(table);
            }

            return tables;
        }

        private string GenerateCreateTableScript(Table table)
        {
            var columnDefinitions = table.Columns.Select(c =>
                $"{c.Name} {TranslateDataType(c.Type)} {GenerateConstraint(c.Constraint)}".Trim());

            return $@"
CREATE TABLE {table.Name} (
    {string.Join(",\n    ", columnDefinitions)}
);";
        }

        private string TranslateDataType(string mermaidType)
        {
            if (mermaidType.StartsWith("string"))
            {
                var match = Regex.Match(mermaidType, @"string\((\d+)\)");
                return match.Success ? $"NVARCHAR({match.Groups[1].Value})" : "NVARCHAR(MAX)";
            }
            if (mermaidType == "int") return "INT";
            if (mermaidType == "DateTime") return "DATETIME";
            if (mermaidType == "bool") return "BIT";
            // 기타 데이터 타입에 대한 변환 로직을 추가하세요.
            return mermaidType.ToUpper(); // 기본적으로 대문자로 변환
        }

        private string GenerateConstraint(string constraint)
        {
            if (constraint == "PK") return "PRIMARY KEY";
            if (constraint == "FK") return ""; // 외래 키는 별도의 ALTER TABLE 문으로 처리해야 할 수 있습니다.
            return constraint ?? "";
        }

        public void ApplyDatabaseScripts(List<DatabaseScript> scripts, DatabaseManager dbManager)
        {
            foreach (var script in scripts)
            {
                dbManager.ExecuteNonQuery(script.DatabaseName, script.Script);
            }
        }
    }

    public class Table
    {
        public string Name { get; set; }
        public List<Column> Columns { get; set; } = new List<Column>();
    }

    public class Column
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Constraint { get; set; }
    }

    public class DatabaseScript
    {
        public string DatabaseName { get; set; }
        public string Script { get; set; }
    }
}