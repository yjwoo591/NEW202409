using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using PC1MAINAITradingSystem.Models.ERD;
using System.ComponentModel.DataAnnotations;

namespace PC1MAINAITradingSystem.Core.ERD.Parser
{
    public partial class ERDParser
    {
        private readonly Dictionary<string, Func<ERDModel, Task<ValidationResult>>> _validators;

        private void InitializeValidators()
        {
            _validators = new Dictionary<string, Func<ERDModel, Task<ValidationResult>>>
            {
                { "Table", ValidateTableStructure },
                { "Column", ValidateColumnDefinitions },
                { "Relationship", ValidateRelationships },
                { "Name", ValidateNameConventions },
                { "DataType", ValidateDataTypes },
                { "Constraint", ValidateConstraints }
            };
        }

        public async Task<ValidationResult> Validate(ERDModel model, IEnumerable<string> specificValidations = null)
        {
            try
            {
                var result = new ValidationResult();

                if (model == null)
                {
                    result.AddError("ERD model is null");
                    return result;
                }

                // 실행할 유효성 검사 선택
                var validationsToRun = specificValidations != null && specificValidations.Any()
                    ? _validators.Where(v => specificValidations.Contains(v.Key))
                    : _validators;

                // 각 유효성 검사 실행
                foreach (var validator in validationsToRun)
                {
                    try
                    {
                        var validationResult = await validator.Value(model);
                        result.MergeWith(validationResult);
                    }
                    catch (Exception ex)
                    {
                        await _logger.LogError($"Validation '{validator.Key}' failed", ex);
                        result.AddError($"Validation '{validator.Key}' failed: {ex.Message}");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                await _logger.LogError("ERD validation failed", ex);
                return new ValidationResult { Errors = { ex.Message } };
            }
        }

        private async Task<ValidationResult> ValidateTableStructure(ERDModel model)
        {
            var result = new ValidationResult();
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

                // 테이블 구조 검증
                if (!table.Columns.Any())
                {
                    result.AddError($"Table {table.Name} must have at least one column");
                }

                if (!table.Columns.Any(c => c.IsPrimaryKey))
                {
                    result.AddWarning($"Table {table.Name} does not have a primary key");
                }
            }

            return result;
        }

        private async Task<ValidationResult> ValidateColumnDefinitions(ERDModel model)
        {
            var result = new ValidationResult();

            foreach (var table in model.Tables)
            {
                var columnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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

                    // 기본키 제약조건 검증
                    if (column.IsPrimaryKey && column.IsNullable)
                    {
                        result.AddError($"Primary key column {column.Name} in table {table.Name} cannot be nullable");
                    }

                    // 외래키 참조 검증
                    if (column.IsForeignKey)
                    {
                        await ValidateForeignKeyReference(model, table, column, result);
                    }
                }
            }

            return result;
        }

        private async Task<ValidationResult> ValidateRelationships(ERDModel model)
        {
            var result = new ValidationResult();

            foreach (var relationship in model.Relationships)
            {
                // 테이블 존재 여부 검증
                var sourceTable = model.Tables.FirstOrDefault(t =>
                    t.Name.Equals(relationship.SourceTable, StringComparison.OrdinalIgnoreCase));
                var targetTable = model.Tables.FirstOrDefault(t =>
                    t.Name.Equals(relationship.TargetTable, StringComparison.OrdinalIgnoreCase));

                if (sourceTable == null)
                {
                    result.AddError($"Source table {relationship.SourceTable} not found");
                    continue;
                }

                if (targetTable == null)
                {
                    result.AddError($"Target table {relationship.TargetTable} not found");
                    continue;
                }

                // 관계 타입 검증
                if (!IsValidRelationType(relationship.RelationType))
                {
                    result.AddError($"Invalid relationship type: {relationship.RelationType}");
                }

                // 순환 참조 검사
                if (await HasCircularReference(model, relationship))
                {
                    result.AddWarning($"Circular reference detected between {relationship.SourceTable} and {relationship.TargetTable}");
                }
            }

            return result;
        }

        private async Task<ValidationResult> ValidateNameConventions(ERDModel model)
        {
            var result = new ValidationResult();
            var namingPattern = new Regex(@"^[a-zA-Z][a-zA-Z0-9_]*$");

            // 테이블 이름 규칙 검사
            foreach (var table in model.Tables)
            {
                if (!namingPattern.IsMatch(table.Name))
                {
                    result.AddWarning($"Table name {table.Name} does not follow naming convention");
                }

                // 컬럼 이름 규칙 검사
                foreach (var column in table.Columns)
                {
                    if (!namingPattern.IsMatch(column.Name))
                    {
                        result.AddWarning($"Column name {column.Name} in table {table.Name} does not follow naming convention");
                    }
                }
            }

            // 관계 이름 규칙 검사
            foreach (var relationship in model.Relationships)
            {
                if (!string.IsNullOrEmpty(relationship.Name) && !namingPattern.IsMatch(relationship.Name))
                {
                    result.AddWarning($"Relationship name {relationship.Name} does not follow naming convention");
                }
            }

            return result;
        }

        private async Task<ValidationResult> ValidateDataTypes(ERDModel model)
        {
            var result = new ValidationResult();

            foreach (var table in model.Tables)
            {
                foreach (var column in table.Columns)
                {
                    // 데이터 타입 기본 검증
                    if (!Enum.IsDefined(typeof(DataType), column.DataType))
                    {
                        result.AddError($"Invalid data type {column.DataType} for column {column.Name} in table {table.Name}");
                        continue;
                    }

                    // 문자열 타입 길이 검증
                    if (IsStringType(column.DataType))
                    {
                        if (!column.Length.HasValue)
                        {
                            result.AddWarning($"String column {column.Name} in table {table.Name} should specify length");
                        }
                        else if (column.Length.Value <= 0 && column.Length.Value != -1)
                        {
                            result.AddError($"Invalid length {column.Length} for column {column.Name} in table {table.Name}");
                        }
                    }

                    // 숫자 타입 정밀도 검증
                    if (IsNumericType(column.DataType))
                    {
                        if (column.Precision.HasValue)
                        {
                            if (column.Precision.Value <= 0 || column.Precision.Value > 38)
                            {
                                result.AddError($"Invalid precision {column.Precision} for column {column.Name} in table {table.Name}");
                            }

                            if (column.Scale.HasValue && column.Scale.Value > column.Precision.Value)
                            {
                                result.AddError($"Scale cannot be greater than precision for column {column.Name} in table {table.Name}");
                            }
                        }
                    }
                }
            }

            return result;
        }

        private async Task<ValidationResult> ValidateConstraints(ERDModel model)
        {
            var result = new ValidationResult();

            foreach (var table in model.Tables)
            {
                // 기본키 제약조건 검증
                var primaryKeys = table.Columns.Where(c => c.IsPrimaryKey).ToList();
                if (primaryKeys.Count > 1)
                {
                    result.AddWarning($"Table {table.Name} has multiple primary keys");
                }

                // 외래키 제약조건 검증
                foreach (var column in table.Columns.Where(c => c.IsForeignKey))
                {
                    if (!await ValidateForeignKeyConstraint(model, table, column))
                    {
                        result.AddError($"Invalid foreign key constraint for column {column.Name} in table {table.Name}");
                    }
                }

                // 기본값 제약조건 검증
                foreach (var column in table.Columns.Where(c => !string.IsNullOrEmpty(c.DefaultValue)))
                {
                    if (!IsValidDefaultValue(column))
                    {
                        result.AddError($"Invalid default value for column {column.Name} in table {table.Name}");
                    }
                }
            }

            return result;
        }

        private bool IsStringType(DataType dataType)
        {
            return dataType is DataType.Varchar or DataType.Nvarchar or DataType.Char or DataType.Nchar;
        }

        private bool IsNumericType(DataType dataType)
        {
            return dataType is DataType.Decimal or DataType.Numeric;
        }

        private async Task<bool> ValidateForeignKeyConstraint(ERDModel model, TableModel table, ColumnModel column)
        {
            var relationship = model.Relationships.FirstOrDefault(r =>
                r.SourceTable == table.Name &&
                r.TargetTable == GetReferencedTableName(column.Name));

            return relationship != null;
        }

        private string GetReferencedTableName(string columnName)
        {
            // 예: CustomerId -> Customer
            return columnName.EndsWith("Id", StringComparison.OrdinalIgnoreCase)
                ? columnName.Substring(0, columnName.Length - 2)
                : columnName;
        }

        private async Task<bool> HasCircularReference(ERDModel model, RelationshipModel relationship,
            HashSet<string> visited = null)
        {
            visited ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!visited.Add(relationship.SourceTable))
            {
                return true;
            }

            var nextRelationships = model.Relationships
                .Where(r => r.SourceTable.Equals(relationship.TargetTable, StringComparison.OrdinalIgnoreCase));

            foreach (var nextRelationship in nextRelationships)
            {
                if (await HasCircularReference(model, nextRelationship, new HashSet<string>(visited)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}