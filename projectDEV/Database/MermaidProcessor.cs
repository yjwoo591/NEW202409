using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ForexAITradingPC1Main.Models;

namespace ForexAITradingPC1Main.Database
{
    public class MermaidProcessor
    {
        public List<TableInfo> ReadMermaidFile(string mermaidContent)
        {
            List<TableInfo> tables = new List<TableInfo>();
            string[] lines = mermaidContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            TableInfo currentTable = null;

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                if (trimmedLine.EndsWith("{"))
                {
                    string tableName = trimmedLine.Split(' ')[0];
                    currentTable = new TableInfo { Name = tableName, Columns = new List<ColumnInfo>() };
                    tables.Add(currentTable);
                }
                else if (trimmedLine == "}")
                {
                    currentTable = null;
                }
                else if (currentTable != null)
                {
                    Match match = Regex.Match(trimmedLine, @"(\w+)\s+(\w+)(?:\s+(\w+))?");
                    if (match.Success)
                    {
                        string dataType = match.Groups[1].Value;
                        string columnName = match.Groups[2].Value;
                        string constraint = match.Groups[3].Success ? match.Groups[3].Value : "";
                        currentTable.Columns.Add(new ColumnInfo
                        {
                         /*   Name = columnName, */
                            DataType = dataType,
                            IsPrimaryKey = constraint.ToUpper() == "PK",
                            IsNullable = !constraint.ToUpper().Contains("NOT NULL")
                        });
                    }
                }
            }

            return tables;
        }

        public string GenerateMermaidContent(List<TableInfo> tables)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("erDiagram");

            foreach (var table in tables)
            {
                sb.AppendLine($"    {table.Name} {{");
                foreach (var column in table.Columns)
                {
                    string constraint = column.IsPrimaryKey ? "PK" : (column.IsNullable ? "" : "NOT NULL");
                    sb.AppendLine($"        {column.DataType} {constraint}".Trim());
                }
                sb.AppendLine("    }");
            }

            return sb.ToString();
        }

        public void AddTableToMermaid(ref string mermaidContent, TableInfo newTable)
        {
            StringBuilder sb = new StringBuilder(mermaidContent);
            sb.AppendLine($"    {newTable.Name} {{");
            foreach (var column in newTable.Columns)
            {
                string constraint = column.IsPrimaryKey ? "PK" : (column.IsNullable ? "" : "NOT NULL");
                sb.AppendLine($"        {column.DataType}  {constraint}".Trim());
            }
            sb.AppendLine("    }");
            mermaidContent = sb.ToString();
        }

        public void RemoveTableFromMermaid(ref string mermaidContent, string tableName)
        {
            string[] lines = mermaidContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sb = new StringBuilder();
            bool skipTable = false;

            foreach (string line in lines)
            {
                if (line.TrimStart().StartsWith($"{tableName} {{"))
                {
                    skipTable = true;
                    continue;
                }

                if (skipTable && line.Trim() == "}")
                {
                    skipTable = false;
                    continue;
                }

                if (!skipTable)
                {
                    sb.AppendLine(line);
                }
            }

            mermaidContent = sb.ToString();
        }

        public void UpdateTableInMermaid(ref string mermaidContent, string oldTableName, TableInfo updatedTable)
        {
            RemoveTableFromMermaid(ref mermaidContent, oldTableName);
            AddTableToMermaid(ref mermaidContent, updatedTable);
        }

        public List<RelationshipInfo> ExtractRelationships(string mermaidContent)
        {
            List<RelationshipInfo> relationships = new List<RelationshipInfo>();
            string[] lines = mermaidContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                Match match = Regex.Match(line, @"(\w+)\s+(.*?)\s+(\w+)\s*:\s*""(.*)""");
                if (match.Success)
                {
                    relationships.Add(new RelationshipInfo
                    {
                        SourceTable = match.Groups[1].Value,
                        Relationship = match.Groups[2].Value,
                        TargetTable = match.Groups[3].Value,
                        Label = match.Groups[4].Value
                    });
                }
            }

            return relationships;
        }

        public void AddRelationshipToMermaid(ref string mermaidContent, RelationshipInfo relationship)
        {
            mermaidContent += $"{relationship.SourceTable} {relationship.Relationship} {relationship.TargetTable} : \"{relationship.Label}\"\n";
        }
    }

    public class RelationshipInfo
    {
        public string SourceTable { get; set; }
        public string Relationship { get; set; }
        public string TargetTable { get; set; }
        public string Label { get; set; }
    }
}
/*이 MermaidProcessor.cs 파일은 다음과 같은 주요 특징을 가지고 있습니다:

Mermaid 형식의 ERD 파일 데이터베이스(ReadMermaidFile)
Mermaid 형식의 ERD 콘텐츠 생성(GenerateMermaidContent)
Mermaid ERD에 테이블 추가 (AddTableToMermaid)
Mermaid ERD에서 테이블 제거 (RemoveTableFromMermaid)
Mermaid ERD의 테이블 업데이트 (UpdateTableInMermaid)
Mermaid ERD에서 관계 추출(ExtractRelationships)
Mermaid ERD에 관계 추가 (AddRelationshipToMermaid)

주요 기능:

ReadMermaidFile: Mermaid 형식의 ERD 내용을 파싱하여 TableInfo가 나타나도록 변환합니다.
generateMermaidContent: TableInfo를 목록으로 변환하세요. Mermaid 형식의 ERD 문자열로 변환합니다.
AddTableToMermaid: 새로운 테이블을 Mermaid ERD에 추가합니다.
RemoveTableFromMermaid: 특정 테이블을 Mermaid ERD에서 제거합니다.
UpdateTableInMermaid: Mermaid ERD의 특정 테이블 정보를 업데이트합니다.
ExtractRelationships: Mermaid ERD에서 테이블 간 관계 정보를 추출합니다.
AddRelationshipToMermaid: 새로운 관계를 Mermaid ERD에 추가합니다.

이 클래스를 사용하여 Mermaid 형식의 ERD를 프로그래밍하여 관리할 수 있습니다. 예를 들어, ERD 편집기에서 사용자 변경가한 내용을 Mermaid 형식으로 변환하거나 데이터베이스 변경 내용을 Mermaid ERD에 문의하는 데 사용할 수 있습니다.
추가 기능이나 수정이 필요하시면 말씀해 주세요.
*/