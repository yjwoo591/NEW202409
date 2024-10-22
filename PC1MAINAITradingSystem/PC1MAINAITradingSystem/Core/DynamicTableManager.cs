```csharp
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PC1MAINAITradingSystem.Interfaces;
using PC1MAINAITradingSystem.Models;

namespace PC1MAINAITradingSystem.Core.DatabaseManager
{
    public class DynamicTableManager : IDynamicTableManager
    {
        private readonly ILogger _logger;
        private readonly IDatabaseManager _dbManager;

        public DynamicTableManager(ILogger logger, IDatabaseManager dbManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbManager = dbManager ?? throw new ArgumentNullException(nameof(dbManager));
        }

        public bool CreateDynamicTable(string tableName, List<ColumnDefinition> columns, string partitionKey = null)
        {
            try
            {
                var columnDefinitions = new List<string>();
                var primaryKeys = new List<string>();

                foreach (var column in columns)
                {
                    var columnDef = BuildColumnDefinition(column);
                    columnDefinitions.Add(columnDef);

                    if (column.IsPrimaryKey)
                    {
                        primaryKeys.Add(column.Name);
                    }
                }

                // 파티션 키가 지정된 경우 추가
                if (!string.IsNullOrEmpty(partitionKey))
                {
                    columnDefinitions.Add($"[PartitionKey] [nvarchar](50) NOT NULL DEFAULT '{partitionKey}'");
                }

                // 기본 키 제약 조건 추가
                if (primaryKeys.Count > 0)
                {
                    var pkConstraint = $"CONSTRAINT [PK_{tableName}] PRIMARY KEY CLUSTERED ([{string.Join("], [", primaryKeys)}])";
                    columnDefinitions.Add(pkConstraint);
                }

                var createTableQuery = $@"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{tableName}]'))
                    BEGIN
                        CREATE TABLE [{tableName}] (
                            {string.Join(",\n            ", columnDefinitions)}
                        )
                    END";

                _dbManager.ExecuteNonQuery(createTableQuery);
                _logger.Log($"Successfully created dynamic table: {tableName}");

                // 파티션 함수 및 스키마 생성 (파티션 키가 있는 경우)
                if (!string.IsNullOrEmpty(partitionKey))
                {
                    CreatePartitionScheme(tableName, partitionKey);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to create dynamic table {tableName}: {ex.Message}");
                throw new DynamicTableCreationException($"Failed to create table {tableName}", ex);
            }
        }

        public bool ModifyDynamicTable(string tableName, List<TableModification> modifications)
        {
            try
            {
                foreach (var modification in modifications)
                {
                    switch (modification.ModificationType)
                    {
                        case TableModificationType.AddColumn:
                            AddColumn(tableName, modification.Column);
                            break;

                        case TableModificationType.ModifyColumn:
                            ModifyColumn(tableName, modification.Column);
                            break;

                        case TableModificationType.RemoveColumn:
                            RemoveColumn(tableName, modification.Column.Name);
                            break;

                        case TableModificationType.AddIndex:
                            CreateIndex(tableName, modification.IndexDefinition);
                            break;

                        case TableModificationType.RemoveIndex:
                            RemoveIndex(tableName, modification.IndexDefinition.Name);
                            break;
                    }
                }

                _logger.Log($"Successfully modified dynamic table: {tableName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to modify dynamic table {tableName}: {ex.Message}");
                throw new DynamicTableModificationException($"Failed to modify table {tableName}", ex);
            }
        }

        public DataTable QueryDynamicTable(string tableName, QueryParameters parameters)
        {
            try
            {
                var queryBuilder = new StringBuilder($"SELECT ");

                // 선택할 컬럼 지정
                if (parameters.Columns != null && parameters.Columns.Count > 0)
                {
                    queryBuilder.Append(string.Join(", ", parameters.Columns));
                }
                else
                {
                    queryBuilder.Append("*");
                }

                queryBuilder.Append($" FROM [{tableName}]");

                // WHERE 절 추가
                if (!string.IsNullOrEmpty(parameters.WhereClause))
                {
                    queryBuilder.Append($" WHERE {parameters.WhereClause}");
                }

                // ORDER BY 절 추가
                if (!string.IsNullOrEmpty(parameters.OrderBy))
                {
                    queryBuilder.Append($" ORDER BY {parameters.OrderBy}");
                }

                // OFFSET/FETCH 절 추가 (페이징)
                if (parameters.PageSize > 0)
                {
                    int offset = (parameters.PageNumber - 1) * parameters.PageSize;
                    queryBuilder.Append($" OFFSET {offset} ROWS FETCH NEXT {parameters.PageSize} ROWS ONLY");
                }

                return _dbManager.ExecuteQuery(queryBuilder.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to query dynamic table {tableName}: {ex.Message}");
                throw new DynamicTableQueryException($"Failed to query table {tableName}", ex);
            }
        }

        private string BuildColumnDefinition(ColumnDefinition column)
        {
            var builder = new StringBuilder($"[{column.Name}] [{column.DataType}]");

            if (column.Length > 0)
            {
                builder.Append($"({column.Length})");
            }

            if (column.IsRequired)
            {
                builder.Append(" NOT NULL");
            }
            else
            {
                builder.Append(" NULL");
            }

            if (column.IsIdentity)
            {
                builder.Append(" IDENTITY(1,1)");
            }

            if (!string.IsNullOrEmpty(column.DefaultValue))
            {
                builder.Append($" DEFAULT {column.DefaultValue}");
            }

            return builder.ToString();
        }

        private void CreatePartitionScheme(string tableName, string partitionKey)
        {
            var createPartitionFunctionQuery = $@"
                IF NOT EXISTS (SELECT * FROM sys.partition_functions WHERE name = 'PF_{tableName}')
                BEGIN
                    CREATE PARTITION FUNCTION [PF_{tableName}](nvarchar(50))
                    AS RANGE RIGHT FOR VALUES ('A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
                                            'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z');

                    CREATE PARTITION SCHEME [PS_{tableName}]
                    AS PARTITION [PF_{tableName}]
                    ALL TO ([PRIMARY]);
                END";

            _dbManager.ExecuteNonQuery(createPartitionFunctionQuery);
            _logger.Log($"Created partition scheme for table: {tableName}");
        }

        private void AddColumn(string tableName, ColumnDefinition column)
        {
            var columnDef = BuildColumnDefinition(column);
            var query = $"ALTER TABLE [{tableName}] ADD {columnDef}";
            _dbManager.ExecuteNonQuery(query);
            _logger.Log($"Added column {column.Name} to table {tableName}");
        }

        private void ModifyColumn(string tableName, ColumnDefinition column)
        {
            var columnDef = BuildColumnDefinition(column);
            var query = $"ALTER TABLE [{tableName}] ALTER COLUMN {columnDef}";
            _dbManager.ExecuteNonQuery(query);
            _logger.Log($"Modified column {column.Name} in table {tableName}");
        }

        private void RemoveColumn(string tableName, string columnName)
        {
            var query = $"ALTER TABLE [{tableName}] DROP COLUMN [{columnName}]";
            _dbManager.ExecuteNonQuery(query);
            _logger.Log($"Removed column {columnName} from table {tableName}");
        }

        private void CreateIndex(string tableName, IndexDefinition indexDef)
        {
            var columns = string.Join(", ", indexDef.Columns.Select(c => $"[{c}]"));
            var unique = indexDef.IsUnique ? "UNIQUE " : "";
            var query = $"CREATE {unique}INDEX [{indexDef.Name}] ON [{tableName}] ({columns})";
            _dbManager.ExecuteNonQuery(query);
            _logger.Log($"Created index {indexDef.Name} on table {tableName}");
        }

        private void RemoveIndex(string tableName, string indexName)
        {
            var query = $"DROP INDEX [{indexName}] ON [{tableName}]";
            _dbManager.ExecuteNonQuery(query);
            _logger.Log($"Removed index {indexName} from table {tableName}");
        }
    }
}
```