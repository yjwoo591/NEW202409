using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using PC1MAINAITradingSystem.Models.ERD;
using System.ComponentModel.DataAnnotations;

namespace PC1MAINAITradingSystem.Core.ERD.Parser
{
    public class MermaidParser : BaseERDParser
    {
        private readonly Dictionary<string, Regex> _patterns;

        public MermaidParser(ILogger logger) : base(logger)
        {
            _patterns = new Dictionary<string, Regex>
            {
                ["TableDefinition"] = new Regex(
                    @"(?<TableName>\w+)\s*{(?<TableContent>[^}]+)}",
                    RegexOptions.Compiled | RegexOptions.Multiline
                ),
                ["ColumnDefinition"] = new Regex(
                    @"^\s*(?<Type>[\w\(\),]+)\s+(?<Name>\w+)(?:\s+(?<Constraints>(?:PK|FK|NULL|NOT NULL|DEFAULT\s+[^,\n]+)(?:\s+(?:PK|FK|NULL|NOT NULL|DEFAULT\s+[^,\n]+))*))?",
                    RegexOptions.Compiled | RegexOptions.Multiline
                ),
                ["Relationship"] = new Regex(
                    @"(?<Source>\w+)\s+(?<Type>[\|\}\{o]-+[\|\}\{o])\s+(?<Target>\w+)(?:\s*:\s*""(?<Name>[^""]+)"")?",
                    RegexOptions.Compiled | RegexOptions.Multiline
                ),
                ["Comment"] = new Regex(
                    @"%%.*?%%|%.*?$",
                    RegexOptions.Compiled | RegexOptions.Multiline
                )
            };
        }

        public override async Task<List<string>> ValidateSyntax(string content)
        {
            var errors = new List<string>();

            try
            {
                if (!await ValidateBasicSyntax(content, errors))
                {
                    return errors;
                }

                // 주석 제거
                content = _patterns["Comment"].Replace(content, "");

                // erDiagram 키워드 확인
                if (!content.Trim().StartsWith("erDiagram"))
                {
                    errors.Add("ERD must start with 'erDiagram' keyword");
                    return errors;
                }

                // 중괄호 매칭 검사
                if (!ValidateBrackets(content))
                {
                    errors.Add("Unmatched brackets found in ERD");
                    return errors;
                }

                // 테이블 정의 구문 검사
                var tableMatches = _patterns["TableDefinition"].Matches(content);
                foreach (Match tableMatch in tableMatches)
                {
                    var tableName = tableMatch.Groups["TableName"].Value;
                    var tableContent = tableMatch.Groups["TableContent"].Value;

                    if (!IsValidIdentifier(tableName))
                    {
                        errors.Add($"Invalid table name: {tableName}");
                    }

                    // 컬럼 정의 구문 검사
                    var columnMatches = _patterns["ColumnDefinition"].Matches(tableContent);
                    foreach (Match columnMatch in columnMatches)
                    {
                        var columnName = columnMatch.Groups["Name"].Value;
                        var columnType = columnMatch.Groups["Type"].Value;

                        if (!IsValidIdentifier(columnName))
                        {
                            errors.Add($"Invalid column name: {columnName} in table {tableName}");
                        }

                        try
                        {
                            var baseType = GetBaseType(columnType);
                            _ = ParseDataType(baseType);
                        }
                        catch (Exception)
                        {
                            errors.Add($"Invalid data type: {columnType} for column {columnName} in table {tableName}");
                        }
                    }
                }

                // 관계 정의 구문 검사
                var relationMatches = _patterns["Relationship"].Matches(content);
                foreach (Match relationMatch in relationMatches)
                {
                    var sourceTable = relationMatch.Groups["Source"].Value;
                    var targetTable = relationMatch.Groups["Target"].Value;
                    var relationType = relationMatch.Groups["Type"].Value;

                    if (!IsValidIdentifier(sourceTable))
                    {
                        errors.Add($"Invalid source table name in relationship: {sourceTable}");
                    }

                    if (!IsValidIdentifier(targetTable))
                    {
                        errors.Add($"Invalid target table name in relationship: {targetTable}");
                    }

                    if (!IsValidRelationType(relationType))
                    {
                        errors.Add($"Invalid relationship type: {relationType}");
                    }
                }

                return errors;
            }
            catch (Exception ex)
            {
                await Logger.LogError("ERD syntax validation failed", ex);
                errors.Add($"Syntax validation failed: {ex.Message}");
                return errors;
            }
        }

        public override async Task<ERDModel> Parse(string content)
        {
            try
            {
                // 주석 제거
                content = _patterns["Comment"].Replace(content, "");

                var model = new ERDModel
                {
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow,
                    Version = "1.0"
                };

                // 테이블 파싱
                var tables = ParseTables(content);
                model.Tables.AddRange(tables);

                // 관계 파싱
                var relationships = ParseRelationships(content);
                model.Relationships.AddRange(relationships);

                await Logger.LogInfo($"Parsed ERD with {model.Tables.Count} tables and {model.Relationships.Count} relationships");
                return model;
            }
            catch (Exception ex)
            {
                await Logger.LogError("ERD parsing failed", ex);
                throw;
            }
        }

        public override async Task<string> Generate(ERDModel model)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("erDiagram");
                sb.AppendLine();

                // 메타데이터 주석
                sb.AppendLine($"%% Generated at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"%% Version: {model.Version}");
                sb.AppendLine();

                // 테이블 정의
                foreach (var table in model.Tables)
                {
                    GenerateTableDefinition(sb, table);
                }

                // 관계 정의
                foreach (var relationship in model.Relationships)
                {
                    GenerateRelationshipDefinition(sb, relationship);
                }

                await Logger.LogInfo("Generated Mermaid ERD");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                await Logger.LogError("ERD generation failed", ex);
                throw;
            }
        }

        private List<TableModel> ParseTables(string content)
        {
            var tables = new List<TableModel>();
            var tableMatches = _patterns["TableDefinition"].Matches(content);

            foreach (Match tableMatch in tableMatches)
            {
                var table = new TableModel
                {
                    Name = tableMatch.Groups["TableName"].Value,
                    Columns = ParseColumns(tableMatch.Groups["TableContent"].Value).ToList()
                };

                tables.Add(table);
            }

            return tables;
        }

        private IEnumerable<ColumnModel> ParseColumns(string content)
        {
            var columnMatches = _patterns["ColumnDefinition"].Matches(content);

            foreach (Match columnMatch in columnMatches)
            {
                var columnType = columnMatch.Groups["Type"].Value;
                var constraints = columnMatch.Groups["Constraints"].Value;

                var (length, precision, scale) = ParseTypeParameters(columnType);
                var baseType = GetBaseType(columnType);

                yield return new ColumnModel
                {
                    Name = columnMatch.Groups["Name"].Value,
                    DataType = ParseDataType(baseType),
                    Length = length,
                    Precision = precision,
                    Scale = scale,
                    IsNullable = !constraints.Contains("NOT NULL"),
                    IsPrimaryKey = constraints.Contains("PK"),
                    IsForeignKey = constraints.Contains("FK"),
                    DefaultValue = ParseDefaultValue(constraints)
                };
            }
        }

        private List<RelationshipModel> ParseRelationships(string content)
        {
            var relationships = new List<RelationshipModel>();
            var relationMatches = _patterns["Relationship"].Matches(content);

            foreach (Match relationMatch in relationMatches)
            {
                var relationship = new RelationshipModel
                {
                    SourceTable = relationMatch.Groups["Source"].Value,
                    TargetTable = relationMatch.Groups["Target"].Value,
                    RelationType = ParseRelationType(relationMatch.Groups["Type"].Value),
                    Name = relationMatch.Groups["Name"].Success ?
                        relationMatch.Groups["Name"].Value :
                        $"FK_{relationMatch.Groups["Source"].Value}_{relationMatch.Groups["Target"].Value}"
                };

                relationships.Add(relationship);
            }

            return relationships;
        }

        private void GenerateTableDefinition(StringBuilder sb, TableModel table)
        {
            sb.AppendLine($"    {table.Name} {{");

            foreach (var column in table.Columns)
            {
                var constraints = new List<string>();
                if (column.IsPrimaryKey) constraints.Add("PK");
                if (column.IsForeignKey) constraints.Add("FK");
                if (!column.IsNullable) constraints.Add("NOT NULL");
                if (!string.IsNullOrEmpty(column.DefaultValue))
                    constraints.Add($"DEFAULT {column.DefaultValue}");

                var typeString = FormatDataType(column);
                var constraintString = constraints.Any() ? " " + string.Join(" ", constraints) : "";

                sb.AppendLine($"        {typeString} {column.Name}{constraintString}");
            }

            sb.AppendLine("    }");
            sb.AppendLine();
        }

        private void GenerateRelationshipDefinition(StringBuilder sb, RelationshipModel relationship)
        {
            var relationSymbol = relationship.RelationType switch
            {
                "1-1" => "||--||",
                "1-n" => "||--|{",
                "n-1" => "}|--||",
                "n-m" => "}|--|{",
                _ => "--"
            };

            var relationshipLine = $"    {relationship.SourceTable} {relationSymbol} {relationship.TargetTable}";

            if (!string.IsNullOrEmpty(relationship.Name))
            {
                relationshipLine += $" : \"{relationship.Name}\"";
            }

            sb.AppendLine(relationshipLine);
        }

        private string ParseDefaultValue(string constraints)
        {
            var match = Regex.Match(constraints, @"DEFAULT\s+([^,\s]+)");
            return match.Success ? match.Groups[1].Value : null;
        }

        private string ParseRelationType(string symbol)
        {
            return symbol switch
            {
                "||--||" => "1-1",
                "||--|{" => "1-n",
                "}|--||" => "n-1",
                "}|--|{" => "n-m",
                _ => "unknown"
            };
        }

        private bool IsValidRelationType(string symbol)
        {
            return symbol is "||--||" or "||--|{" or "}|--||" or "}|--|{";
        }

        private string FormatDataType(ColumnModel column)
        {
            return column.DataType switch
            {
                DataType.Varchar or DataType.Nvarchar or DataType.Char or DataType.Nchar
                    when column.Length.HasValue => $"{column.DataType}({column.Length})",

                DataType.Decimal or DataType.Numeric
                    when column.Precision.HasValue && column.Scale.HasValue =>
                    $"{column.DataType}({column.Precision},{column.Scale})",

                _ => column.DataType.ToString().ToLower()
            };
        }

        private bool ValidateBrackets(string content)
        {
            var stack = new Stack<char>();
            var brackets = new Dictionary<char, char> { { '{', '}' }, { '(', ')' }, { '[', ']' } };

            foreach (char c in content)
            {
                if (brackets.ContainsKey(c))
                {
                    stack.Push(c);
                }
                else if (brackets.ContainsValue(c))
                {
                    if (stack.Count == 0) return false;

                    var lastOpen = stack.Pop();
                    if (brackets[lastOpen] != c) return false;
                }
            }

            return stack.Count == 0;
        }
    }
}