```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using PC1MAINAITradingSystem.Interfaces;
using PC1MAINAITradingSystem.Models;

namespace PC1MAINAITradingSystem.Core.ERDProcessor
{
    public class ERDValidator : IERDValidator
    {
        private readonly ILogger _logger;
        private readonly List<string> _errors;
        private readonly List<string> _warnings;

        public ERDValidator(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _errors = new List<string>();
            _warnings = new List<string>();
        }

        public ValidationResult ValidateERD(ERDStructure erdStructure)
        {
            try
            {
                _logger.Log("Starting ERD validation");
                _errors.Clear();
                _warnings.Clear();

                ValidateTables(erdStructure.Tables);
                ValidateRelations(erdStructure.Relations, erdStructure.Tables);

                var result = new ValidationResult
                {
                    IsValid = _errors.Count == 0,
                    ErrorMessages = _errors.ToList(),
                    WarningMessages = _warnings.ToList()
                };

                _logger.Log($"ERD validation completed. Valid: {result.IsValid}, Errors: {result.ErrorMessages.Count}, Warnings: {result.WarningMessages.Count}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during ERD validation: {ex.Message}");
                throw new ERDValidationException("Failed to validate ERD structure", ex);
            }
        }

        private void ValidateTables(List<TableStructure> tables)
        {
            if (tables == null || tables.Count == 0)
            {
                _errors.Add("ERD must contain at least one table");
                return;
            }

            // 테이블 이름 중복 검사
            var duplicateTableNames = tables
                .GroupBy(t => t.Name.ToUpper())
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            foreach (var duplicate in duplicateTableNames)
            {
                _errors.Add($"Duplicate table name found: {duplicate}");
            }

            // 각 테이블 검증
            foreach (var table in tables)
            {
                ValidateTable(table);
            }
        }

        private void ValidateTable(TableStructure table)
        {
            if (string.IsNullOrWhiteSpace(table.Name))
            {
                _errors.Add("Table name cannot be empty");
                return;
            }

            if (!IsValidTableName(table.Name))
            {
                _errors.Add($"Invalid table name: {table.Name}. Table names must start with a letter and contain only letters, numbers, and underscores");
            }

            if (table.Columns == null || table.Columns.Count == 0)
            {
                _errors.Add($"Table {table.Name} must contain at least one column");
                return;
            }

            // 테이블 내 컬럼 이름 중복 검사
            var duplicateColumnNames = table.Columns
                .GroupBy(c => c.Name.ToUpper())
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            foreach (var duplicate in duplicateColumnNames)
            {
                _errors.Add($"Duplicate column name found in table {table.Name}: {duplicate}");
            }

            // 각 컬럼 검증
            foreach (var column in table.Columns)
            {
                ValidateColumn(column, table.Name);
            }

            // 기본 키 검증
            ValidatePrimaryKey(table);
        }

        private void ValidateColumn(ColumnStructure column, string tableName)
        {
            if (string.IsNullOrWhiteSpace(column.Name))
            {
                _errors.Add($"Column name cannot be empty in table {tableName}");
                return;
            }

            if (!IsValidColumnName(column.Name))
            {
                _errors.Add($"Invalid column name in table {tableName}: {column.Name}. Column names must start with a letter and contain only letters, numbers, and underscores");
            }

            if (string.IsNullOrWhiteSpace(column.DataType))
            {
                _errors.Add($"Data type must be specified for column {column.Name} in table {tableName}");
                return;
            }

            if (!IsValidDataType(column.DataType))
            {
                _errors.Add($"Invalid data type for column {column.Name} in table {tableName}: {column.DataType}");
            }
        }

        private void ValidatePrimaryKey(TableStructure table)
        {
            var primaryKeyColumns = table.Columns
                .Where(c => c.Properties.ContainsKey("PrimaryKey") &&
                           c.Properties["PrimaryKey"].Equals("true", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (primaryKeyColumns.Count == 0)
            {
                _errors.Add($"Table {table.Name} must have at least one primary key column");
            }
        }

        private void ValidateRelations(List<TableRelation> relations, List<TableStructure> tables)
        {
            if (relations == null)
            {
                return;
            }

            foreach (var relation in relations)
            {
                ValidateRelation(relation, tables);
            }
        }

        private void ValidateRelation(TableRelation relation, List<TableStructure> tables)
        {
            // 소스 테이블 존재 여부 확인
            var sourceTable = tables.FirstOrDefault(t => t.Name == relation.SourceTable);
            if (sourceTable == null)
            {
                _errors.Add($"Source table not found for relation: {relation.SourceTable}");
                return;
            }

            // 타겟 테이블 존재 여부 확인
            var targetTable = tables.FirstOrDefault(t => t.Name == relation.TargetTable);
            if (targetTable == null)
            {
                _errors.Add($"Target table not found for relation: {relation.TargetTable}");
                return;
            }

            // 소스 컬럼 존재 여부 확인
            if (!sourceTable.Columns.Any(c => c.Name == relation.SourceColumn))
            {
                _errors.Add($"Source column {relation.SourceColumn} not found in table {relation.SourceTable}");
            }

            // 타겟 컬럼 존재 여부 확인
            if (!targetTable.Columns.Any(c => c.Name == relation.TargetColumn))
            {
                _errors.Add($"Target column {relation.TargetColumn} not found in table {relation.TargetTable}");
            }
        }

        private bool IsValidTableName(string name)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z][a-zA-Z0-9_]*$");
        }

        private bool IsValidColumnName(string name)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z][a-zA-Z0-9_]*$");
        }

        private bool IsValidDataType(string dataType)
        {
            var validTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "int", "bigint", "smallint", "tinyint",
                "decimal", "numeric", "float", "real",
                "char", "varchar", "nchar", "nvarchar", "text",
                "date", "datetime", "datetime2", "time",
                "bit", "binary", "varbinary",
                "uniqueidentifier"
            };

            // 기본 타입 체크
            if (validTypes.Contains(dataType))
            {
                return true;
            }

            // 크기가 지정된 타입 체크 (예: varchar(50))
            var match = System.Text.RegularExpressions.Regex.Match(dataType, @"^(varchar|nvarchar|char|nchar)\(\d+\)$");
            return match.Success;
        }
    }
}
```