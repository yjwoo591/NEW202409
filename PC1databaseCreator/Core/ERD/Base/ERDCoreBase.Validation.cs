using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using PC1MAINAITradingSystem.Models.ERD;
using System.ComponentModel.DataAnnotations;

namespace PC1MAINAITradingSystem.Core.ERD.Base
{
    public partial class ERDCoreBase
    {
        public async Task<ValidationResult> ValidateERD(ERDModel model)
        {
            try
            {
                if (!_isInitialized)
                {
                    throw new InvalidOperationException("ERD Core is not initialized");
                }

                var result = new ValidationResult();

                // 기본 검증
                if (!ValidateBasicRequirements(model, result))
                {
                    return result;
                }

                // 테이블 검증
                await ValidateTables(model, result);

                // 관계 검증
                await ValidateRelationships(model, result);

                // 순환 참조 검사
                await ValidateCircularReferences(model, result);

                // 이름 중복 검사
                await ValidateDuplicateNames(model, result);

                await _logger.LogInfo($"ERD validation completed with {result.Errors.Count} errors and {result.Warnings.Count} warnings");
                return result;
            }
            catch (Exception ex)
            {
                await _logger.LogError("ERD validation failed", ex);
                throw;
            }
        }

        private bool ValidateBasicRequirements(ERDModel model, ValidationResult result)
        {
            if (model == null)
            {
                result.AddError("ERD model is null");
                return false;
            }

            if (!model.Tables.Any())
            {
                result.AddError("ERD must contain at least one table");
                return false;
            }

            return true;
        }

        private async Task ValidateTables(ERDModel model, ValidationResult result)
        {
            var tableNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var table in model.Tables)
            {
                // 테이블 이름 검증
                if (string.IsNullOrWhiteSpace(table.Name))
                {
                    result.AddError("Table name cannot be empty");
                    continue;
                }

                if (!IsValidIdentifier(table.Name))
                {
                    result.AddError($"Invalid table name: {table.Name}");
                    continue;
                }

                if (!tableNames.Add(table.Name))
                {
                    result.AddError($"Duplicate table name: {table.Name}");
                    continue;
                }

                // 컬럼 검증
                await ValidateColumns(table, result);

                // 인덱스 검증
                await ValidateIndexes(table, result);

                // 제약조건 검증
                await ValidateConstraints(table, result);
            }
        }

        private async Task ValidateColumns(TableModel table, ValidationResult result)
        {
            var columnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var hasPrimaryKey = false;

            foreach (var column in table.Columns)
            {
                // 컬럼 이름 검증
                if (string.IsNullOrWhiteSpace(column.Name))
                {
                    result.AddError($"Column name cannot be empty in table {table.Name}");
                    continue;
                }

                if (!IsValidIdentifier(column.Name))
                {
                    result.AddError($"Invalid column name: {column.Name} in table {table.Name}");
                    continue;
                }

                if (!columnNames.Add(column.Name))
                {
                    result.AddError($"Duplicate column name: {column.Name} in table {table.Name}");
                    continue;
                }

                // 데이터 타입 검증
                if (!IsValidDataType(column.DataType))
                {
                    result.AddError($"Invalid data type: {column.DataType} for column {column.Name} in table {table.Name}");
                }

                // 길이 검증
                if (RequiresLength(column.DataType) && !column.Length.HasValue)
                {
                    result.AddWarning($"Length should be specified for {column.DataType} column {column.Name} in table {table.Name}");
                }

                // 정밀도 검증
                if (RequiresPrecision(column.DataType))
                {
                    if (!column.Precision.HasValue || !column.Scale.HasValue)
                    {
                        result.AddWarning($"Precision and scale should be specified for {column.DataType} column {column.Name} in table {table.Name}");
                    }
                }

                // 기본키 검증
                if (column.IsPrimaryKey)
                {
                    hasPrimaryKey = true;
                    if (column.IsNullable)
                    {
                        result.AddError($"Primary key column {column.Name} in table {table.Name} cannot be nullable");
                    }
                }

                // 기본값 검증
                if (!string.IsNullOrEmpty(column.DefaultValue))
                {
                    if (!IsValidDefaultValue(column))
                    {
                        result.AddError($"Invalid default value for column {column.Name} in table {table.Name}");
                    }
                }
            }

            if (!hasPrimaryKey)
            {
                result.AddWarning($"Table {table.Name} does not have a primary key");
            }
        }

        private async Task ValidateIndexes(TableModel table, ValidationResult result)
        {
            var indexNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var hasClustered = false;

            foreach (var index in table.Indexes)
            {
                // 인덱스 이름 검증
                if (string.IsNullOrWhiteSpace(index.Name))
                {
                    result.AddError($"Index name cannot be empty in table {table.Name}");
                    continue;
                }

                if (!indexNames.Add(index.Name))
                {
                    result.AddError($"Duplicate index name: {index.Name} in table {table.Name}");
                    continue;
                }

                // 컬럼 존재 여부 검증
                foreach (var columnName in index.Columns)
                {
                    if (!table.Columns.Any(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase)))
                    {
                        result.AddError($"Index {index.Name} references non-existent column {columnName} in table {table.Name}");
                    }
                }

                // 클러스터드 인덱스 중복 검사
                if (index.IsClustered)
                {
                    if (hasClustered)
                    {
                        result.AddError($"Multiple clustered indexes found in table {table.Name}");
                    }
                    hasClustered = true;
                }
            }
        }

        private async Task ValidateRelationships(ERDModel model, ValidationResult result)
        {
            foreach (var relationship in model.Relationships)
            {
                // 테이블 존재 여부 검증
                var sourceTable = model.Tables.FirstOrDefault(t =>
                    t.Name.Equals(relationship.SourceTable, StringComparison.OrdinalIgnoreCase));
                var targetTable = model.Tables.FirstOrDefault(t =>
                    t.Name.Equals(relationship.TargetTable, StringComparison.OrdinalIgnoreCase));

                if (sourceTable == null)
                {
                    result.AddError($"Source table {relationship.SourceTable} not found for relationship");
                    continue;
                }

                if (targetTable == null)
                {
                    result.AddError($"Target table {relationship.TargetTable} not found for relationship");
                    continue;
                }

                // 관계 타입 검증
                if (!IsValidRelationType(relationship.RelationType))
                {
                    result.AddError($"Invalid relationship type: {relationship.RelationType}");
                }

                // 외래키 컬럼 검증
                if (!HasValidForeignKeyColumn(sourceTable, targetTable))
                {
                    result.AddWarning($"No valid foreign key column found in {sourceTable.Name} referencing {targetTable.Name}");
                }
            }
        }

        private async Task ValidateCircularReferences(ERDModel model, ValidationResult result)
        {
            var visited = new HashSet<string>();
            var recursionStack = new HashSet<string>();

            foreach (var table in model.Tables)
            {
                if (!visited.Contains(table.Name))
                {
                    if (await HasCircularReference(model, table.Name, visited, recursionStack))
                    {
                        result.AddError($"Circular reference detected starting from table {table.Name}");
                    }
                }
            }
        }

        private async Task ValidateConstraints(TableModel table, ValidationResult result)
        {
            // 제약조건 검증 로직 구현
            await Task.CompletedTask;
        }

        private async Task ValidateDuplicateNames(ERDModel model, ValidationResult result)
        {
            // 중복 이름 검증 로직 구현
            await Task.CompletedTask;
        }

        private bool IsValidIdentifier(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;

            if (!char.IsLetter(name[0]) && name[0] != '_')
                return false;

            return name.All(c => char.IsLetterOrDigit(c) || c == '_');
        }

        private bool IsValidDataType(DataType dataType)
        {
            return Enum.IsDefined(typeof(DataType), dataType);
        }

        private bool RequiresLength(DataType dataType)
        {
            return dataType is DataType.Varchar or DataType.Nvarchar or DataType.Char or DataType.Nchar;
        }

        private bool RequiresPrecision(DataType dataType)
        {
            return dataType is DataType.Decimal or DataType.Numeric;
        }

        private bool IsValidDefaultValue(ColumnModel column)
        {
            if (string.IsNullOrEmpty(column.DefaultValue)) return true;

            try
            {
                return column.DataType switch
                {
                    DataType.Int => int.TryParse(column.DefaultValue, out _),
                    DataType.Bigint => long.TryParse(column.DefaultValue, out _),
                    DataType.Decimal or DataType.Numeric => decimal.TryParse(column.DefaultValue, out _),
                    DataType.Bool => bool.TryParse(column.DefaultValue, out _),
                    DataType.Datetime => DateTime.TryParse(column.DefaultValue, out _),
                    _ => true // 문자열 타입은 모든 값 허용
                };
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidRelationType(string relationType)
        {
            return relationType is "1-1" or "1-n" or "n-1" or "n-m";
        }

        private bool HasValidForeignKeyColumn(TableModel sourceTable, TableModel targetTable)
        {
            var expectedColumnName = $"{targetTable.Name}Id";
            return sourceTable.Columns.Any(c =>
                c.Name.Equals(expectedColumnName, StringComparison.OrdinalIgnoreCase) &&
                c.IsForeignKey);
        }

        private async Task<bool> HasCircularReference(
            ERDModel model,
            string tableName,
            HashSet<string> visited,
            HashSet<string> recursionStack)
        {
            visited.Add(tableName);
            recursionStack.Add(tableName);

            var relationships = model.Relationships
                .Where(r => r.SourceTable.Equals(tableName, StringComparison.OrdinalIgnoreCase));

            foreach (var relationship in relationships)
            {
                if (!visited.Contains(relationship.TargetTable))
                {
                    if (await HasCircularReference(model, relationship.TargetTable, visited, recursionStack))
                    {
                        return true;
                    }
                }
                else if (recursionStack.Contains(relationship.TargetTable))
                {
                    return true;
                }
            }

            recursionStack.Remove(tableName);
            return false;
        }
    }
}