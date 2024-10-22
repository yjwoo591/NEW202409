```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using PC1MAINAITradingSystem.Interfaces;
using PC1MAINAITradingSystem.Models;

namespace PC1MAINAITradingSystem.Core.ERDProcessor
{
    public class ERDParser : IERDParser
    {
        private readonly ILogger _logger;

        public ERDParser(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ERDStructure ParseERD(string erdContent)
        {
            try
            {
                _logger.Log("Starting ERD parsing");
                var erdStructure = new ERDStructure
                {
                    Tables = new List<TableStructure>(),
                    Relations = new List<TableRelation>()
                };

                var lines = erdContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                ParseTables(lines, erdStructure);
                ParseRelations(lines, erdStructure);

                _logger.Log($"ERD parsing completed. Found {erdStructure.Tables.Count} tables and {erdStructure.Relations.Count} relations");
                return erdStructure;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error parsing ERD: {ex.Message}");
                throw new ERDParseException("Failed to parse ERD content", ex);
            }
        }

        private void ParseTables(string[] lines, ERDStructure erdStructure)
        {
            foreach (var line in lines.Where(l => l.StartsWith("TABLE:")))
            {
                var tableStructure = ParseTableDefinition(line);
                erdStructure.Tables.Add(tableStructure);
            }
        }

        private TableStructure ParseTableDefinition(string tableLine)
        {
            try
            {
                // Format: TABLE: TableName (Column1:Type1:Props1, Column2:Type2:Props2, ...)
                var parts = tableLine.Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                var tableName = parts[0].Replace("TABLE:", "").Trim();

                var tableStructure = new TableStructure
                {
                    Name = tableName,
                    Columns = new List<ColumnStructure>()
                };

                if (parts.Length > 1)
                {
                    var columnDefinitions = parts[1].Split(',');
                    foreach (var columnDef in columnDefinitions)
                    {
                        var columnStructure = ParseColumnDefinition(columnDef.Trim());
                        tableStructure.Columns.Add(columnStructure);
                    }
                }

                return tableStructure;
            }
            catch (Exception ex)
            {
                throw new ERDParseException($"Error parsing table definition: {tableLine}", ex);
            }
        }

        private ColumnStructure ParseColumnDefinition(string columnDef)
        {
            try
            {
                // Format: ColumnName:DataType:Properties
                var parts = columnDef.Split(':');
                if (parts.Length < 2)
                {
                    throw new ERDParseException($"Invalid column definition format: {columnDef}");
                }

                var columnStructure = new ColumnStructure
                {
                    Name = parts[0].Trim(),
                    DataType = parts[1].Trim(),
                    Properties = new Dictionary<string, string>()
                };

                if (parts.Length > 2)
                {
                    ParseColumnProperties(parts[2], columnStructure.Properties);
                }

                return columnStructure;
            }
            catch (Exception ex)
            {
                throw new ERDParseException($"Error parsing column definition: {columnDef}", ex);
            }
        }

        private void ParseColumnProperties(string propertiesStr, Dictionary<string, string> properties)
        {
            var props = propertiesStr.Split(';');
            foreach (var prop in props)
            {
                var keyValue = prop.Split('=');
                if (keyValue.Length == 2)
                {
                    properties[keyValue[0].Trim()] = keyValue[1].Trim();
                }
            }
        }

        private void ParseRelations(string[] lines, ERDStructure erdStructure)
        {
            foreach (var line in lines.Where(l => l.StartsWith("RELATION:")))
            {
                var relation = ParseRelationDefinition(line);
                erdStructure.Relations.Add(relation);
            }
        }

        private TableRelation ParseRelationDefinition(string relationLine)
        {
            try
            {
                // Format: RELATION: Table1:Column1 -> Table2:Column2 (RelationType)
                var parts = relationLine.Replace("RELATION:", "").Trim().Split(new[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
                var sourceInfo = parts[0].Trim().Split(':');
                var targetInfo = parts[1].Split('(')[0].Trim().Split(':');
                var relationType = parts[1].Split('(')[1].TrimEnd(')');

                return new TableRelation
                {
                    SourceTable = sourceInfo[0],
                    SourceColumn = sourceInfo[1],
                    TargetTable = targetInfo[0],
                    TargetColumn = targetInfo[1],
                    RelationType = ParseRelationType(relationType)
                };
            }
            catch (Exception ex)
            {
                throw new ERDParseException($"Error parsing relation definition: {relationLine}", ex);
            }
        }

        private RelationType ParseRelationType(string relationType)
        {
            return relationType.ToUpper() switch
            {
                "1:1" => RelationType.OneToOne,
                "1:N" => RelationType.OneToMany,
                "N:1" => RelationType.ManyToOne,
                "N:N" => RelationType.ManyToMany,
                _ => throw new ERDParseException($"Unknown relation type: {relationType}")
            };
        }
    }
}
```