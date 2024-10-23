using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using PC1MAINAITradingSystem.Core.ERD.Models;
using System.ComponentModel.DataAnnotations;

namespace PC1MAINAITradingSystem.Core.Database
{
    public class DatabaseSchemaManager
    {
        private readonly DatabaseManager _databaseManager;
        private readonly ILogger _logger;
        private readonly ISecurityManager _securityManager;

        public DatabaseSchemaManager(
            DatabaseManager databaseManager,
            ILogger logger,
            ISecurityManager securityManager)
        {
            _databaseManager = databaseManager ?? throw new ArgumentNullException(nameof(databaseManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _securityManager = securityManager ?? throw new ArgumentNullException(nameof(securityManager));
        }

        public async Task<DatabaseOperationResult> ApplySchemaChanges(ERDModel model, SchemaUpdateOptions options)
        {
            try
            {
                await _logger.LogInfo("Starting schema changes application");

                // 권한 검증
                if (!await ValidatePermissions())
                {
                    throw new UnauthorizedAccessException("Insufficient permissions for schema modification");
                }

                // 현재 스키마 추출
                var currentSchema = await ExtractCurrentSchema();

                // 변경사항 분석
                var changes = await AnalyzeSchemaChanges(currentSchema, model);

                // 변경 전 백업 수행
                if (options.CreateBackupBeforeChanges)
                {
                    await CreateSchemaBackup();
                }

                // 변경사항 적용
                var commands = await GenerateSchemaUpdateCommands(changes, options);
                var result = await ExecuteSchemaChanges(commands, options);

                if (result.Success)
                {
                    await _logger.LogInfo("Schema changes applied successfully");
                }

                return result;
            }
            catch (Exception ex)
            {
                await _logger.LogError("Failed to apply schema changes", ex);
                return new DatabaseOperationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ERDModel> ExtractCurrentSchema()
        {
            try
            {
                await _logger.LogInfo("Starting current schema extraction");

                var model = new ERDModel
                {
                    Version = "1.0",
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };

                // 테이블 정보 추출
                model.Tables = await ExtractTables();

                // 관계 정보 추출
                model.Relationships = await ExtractRelationships();

                await _logger.LogInfo($"Schema extraction completed. Found {model.Tables.Count} tables and {model.Relationships.Count} relationships");
                return model;
            }
            catch (Exception ex)
            {
                await _logger.LogError("Schema extraction failed", ex);
                throw;
            }
        }

        private async Task<List<TableModel>> ExtractTables()
        {
            var tables = new List<TableModel>();

            var query = @"
                SELECT 
                    t.name AS TableName,
                    SCHEMA_NAME(t.schema_id) AS SchemaName,
                    p.value AS TableDescription
                FROM sys.tables t
                LEFT JOIN sys.extended_properties p ON
                    p.major_id = t.object_id AND
                    p.minor_id = 0 AND
                    p.name = 'MS_Description'
                ORDER BY TableName";

            var result = await _databaseManager.ExecuteQuery(query);
            if (!result.Success) throw new Exception("Failed to extract tables");

            foreach (var row in result.Data)
            {
                var table = new TableModel
                {
                    Name = row["TableName"].ToString(),
                    Description = row["TableDescription"]?.ToString(),
                    Columns = await ExtractColumns(row["TableName"].ToString()),
                    Indexes = await ExtractIndexes(row["TableName"].ToString())
                };

                tables.Add(table);
            }

            return tables;
        }

        private async Task<List<ColumnModel>> ExtractColumns(string tableName)
        {
            var columns = new List<ColumnModel>();

            var query = @"
                SELECT 
                    c.name AS ColumnName,
                    t.name AS DataType,
                    c.max_length AS MaxLength,
                    c.precision AS Precision,
                    c.scale AS Scale,
                    c.is_nullable AS IsNullable,
                    CASE WHEN pk.column_id IS NOT NULL THEN 1 ELSE 0 END AS IsPrimaryKey,
                    CASE WHEN fk.parent_column_id IS NOT NULL THEN 1 ELSE 0 END AS IsForeignKey,
                    dc.definition AS DefaultValue,
                    ep.value AS Description
                FROM sys.columns c
                INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                LEFT JOIN sys.extended_properties ep ON 
                    ep.major_id = c.object_id AND 
                    ep.minor_id = c.column_id AND 
                    ep.name = 'MS_Description'
                LEFT JOIN sys.default_constraints dc ON 
                    dc.parent_object_id = c.object_id AND 
                    dc.parent_column_id = c.column_id
                LEFT JOIN (
                    SELECT ic.object_id, ic.column_id
                    FROM sys.indexes i
                    JOIN sys.index_columns ic ON i.object_id = ic.object_id
                    WHERE i.is_primary_key = 1
                ) pk ON c.object_id = pk.object_id AND c.column_id = pk.column_id
                LEFT JOIN sys.foreign_key_columns fk ON 
                    c.object_id = fk.parent_object_id AND 
                    c.column_id = fk.parent_column_id
                WHERE c.object_id = OBJECT_ID(@TableName)
                ORDER BY c.column_id";

            var parameters = new Dictionary<string, object>
            {
                { "@TableName", tableName }
            };

            var result = await _databaseManager.ExecuteQuery(query, parameters);
            if (!result.Success) throw new Exception($"Failed to extract columns for table {tableName}");

            foreach (var row in result.Data)
            {
                var column = new ColumnModel
                {
                    Name = row["ColumnName"].ToString(),
                    DataType = ParseDataType(row["DataType"].ToString()),
                    Length = Convert.ToInt32(row["MaxLength"]),
                    Precision = Convert.ToInt32(row["Precision"]),
                    Scale = Convert.ToInt32(row["Scale"]),
                    IsNullable = Convert.ToBoolean(row["IsNullable"]),
                    IsPrimaryKey = Convert.ToBoolean(row["IsPrimaryKey"]),
                    IsForeignKey = Convert.ToBoolean(row["IsForeignKey"]),
                    DefaultValue = row["DefaultValue"]?.ToString(),
                    Description = row["Description"]?.ToString()
                };

                columns.Add(column);
            }

            return columns;
        }

        private async Task<List<IndexModel>> ExtractIndexes(string tableName)
        {
            var indexes = new List<IndexModel>();

            var query = @"
                SELECT 
                    i.name AS IndexName,
                    i.is_unique AS IsUnique,
                    i.is_primary_key AS IsPrimaryKey,
                    i.type_desc AS IndexType,
                    STRING_AGG(c.name, ',') WITHIN GROUP (ORDER BY ic.key_ordinal) AS Columns
                FROM sys.indexes i
                INNER JOIN sys.index_columns ic ON 
                    i.object_id = ic.object_id AND 
                    i.index_id = ic.index_id
                INNER JOIN sys.columns c ON 
                    ic.object_id = c.object_id AND 
                    ic.column_id = c.column_id
                WHERE i.object_id = OBJECT_ID(@TableName)
                    AND i.is_primary_key = 0
                    AND i.is_unique_constraint = 0
                GROUP BY 
                    i.name,
                    i.is_unique,
                    i.is_primary_key,
                    i.type_desc";

            var parameters = new Dictionary<string, object>
            {
                { "@TableName", tableName }
            };

            var result = await _databaseManager.ExecuteQuery(query, parameters);
            if (!result.Success) throw new Exception($"Failed to extract indexes for table {tableName}");

            foreach (var row in result.Data)
            {
                var index = new IndexModel
                {
                    Name = row["IndexName"].ToString(),
                    IsUnique = Convert.ToBoolean(row["IsUnique"]),
                    IsClustered = row["IndexType"].ToString() == "CLUSTERED",
                    Columns = row["Columns"].ToString().Split(',').ToList()
                };

                indexes.Add(index);
            }

            return indexes;
        }

        private async Task<List<RelationshipModel>> ExtractRelationships()
        {
            var relationships = new List<RelationshipModel>();

            var query = @"
                SELECT 
                    fk.name AS ForeignKeyName,
                    OBJECT_NAME(fk.parent_object_id) AS SourceTable,
                    OBJECT_NAME(fk.referenced_object_id) AS TargetTable,
                    dc.name AS DeleteRule,
                    uc.name AS UpdateRule
                FROM sys.foreign_keys fk
                LEFT JOIN sys.foreign_key_columns fkc ON 
                    fk.object_id = fkc.constraint_object_id
                LEFT JOIN sys.objects dc ON 
                    fk.delete_referential_action_desc = dc.name
                LEFT JOIN sys.objects uc ON 
                    fk.update_referential_action_desc = uc.name";

            var result = await _databaseManager.ExecuteQuery(query);
            if (!result.Success) throw new Exception("Failed to extract relationships");

            foreach (var row in result.Data)
            {
                var relationship = new RelationshipModel
                {
                    Name = row["ForeignKeyName"].ToString(),
                    SourceTable = row["SourceTable"].ToString(),
                    TargetTable = row["TargetTable"].ToString(),
                    RelationType = "1-n", // 기본값으로 1-n 관계 설정
                    OnDelete = ParseReferentialAction(row["DeleteRule"]?.ToString()),
                    OnUpdate = ParseReferentialAction(row["UpdateRule"]?.ToString())
                };

                relationships.Add(relationship);
            }

            return relationships;
        }

        private async Task<SchemaChanges> AnalyzeSchemaChanges(ERDModel currentSchema, ERDModel targetSchema)
        {
            var changes = new SchemaChanges();

            // 테이블 변경사항 분석
            foreach (var targetTable in targetSchema.Tables)
            {
                var currentTable = currentSchema.Tables
                    .FirstOrDefault(t => t.Name.Equals(targetTable.Name, StringComparison.OrdinalIgnoreCase));

                if (currentTable == null)
                {
                    changes.AddedTables.Add(targetTable);
                }
                else
                {
                    var tableChanges = AnalyzeTableChanges(currentTable, targetTable);
                    if (tableChanges.HasChanges)
                    {
                        changes.ModifiedTables.Add(tableChanges);
                    }
                }
            }

            // 삭제된 테이블 확인
            foreach (var currentTable in currentSchema.Tables)
            {
                if (!targetSchema.Tables.Any(t =>
                    t.Name.Equals(currentTable.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    changes.RemovedTables.Add(currentTable);
                }
            }

            // 관계 변경사항 분석
            foreach (var targetRelation in targetSchema.Relationships)
            {
                var currentRelation = currentSchema.Relationships
                    .FirstOrDefault(r =>
                        r.SourceTable.Equals(targetRelation.SourceTable, StringComparison.OrdinalIgnoreCase) &&
                        r.TargetTable.Equals(targetRelation.TargetTable, StringComparison.OrdinalIgnoreCase));

                if (currentRelation == null)
                {
                    changes.AddedRelationships.Add(targetRelation);
                }
                else if (!RelationshipsAreEqual(currentRelation, targetRelation))
                {
                    changes.ModifiedRelationships.Add((currentRelation, targetRelation));
                }
            }

            // 삭제된 관계 확인
            foreach (var currentRelation in currentSchema.Relationships)
            {
                if (!targetSchema.Relationships.Any(r =>
                    r.SourceTable.Equals(currentRelation.SourceTable, StringComparison.OrdinalIgnoreCase) &&
                    r.TargetTable.Equals(currentRelation.TargetTable, StringComparison.OrdinalIgnoreCase)))
                {
                    changes.RemovedRelationships.Add(currentRelation);
                }
            }

            return changes;
        }

        private async Task<List<DatabaseCommand>> GenerateSchemaUpdateCommands(
            SchemaChanges changes, SchemaUpdateOptions options)
        {
            var commands = new List<DatabaseCommand>();

            // 테이블 삭제 명령 생성
            if (options.AllowTableDrop)
            {
                foreach (var table in changes.RemovedTables)
                {
                    commands.Add(new DatabaseCommand
                    {
                        Query = $"DROP TABLE [{table.Name}]"
                    });
                }
            }

            // 새 테이블 생성 명령 생성
            foreach (var table in changes.AddedTables)
            {
                commands.Add(GenerateCreateTableCommand(table));
            }

            // 테이블 수정 명령 생성
            foreach (var tableChange in changes.ModifiedTables)
            {
                commands.AddRange(GenerateAlterTableCommands(tableChange));
            }

            // 관계 삭제 명령 생성
            foreach (var relationship in changes.RemovedRelationships)
            {
                commands.Add(new DatabaseCommand
                {
                    Query = $"ALTER TABLE [{relationship.SourceTable}] DROP CONSTRAINT [{relationship.Name}]"
                });
            }

            // 새 관계 생성 명령 생성
            foreach (var relationship in changes.AddedRelationships)
            {
                commands.Add(GenerateCreateRelationshipCommand(relationship));
            }

            // 관계 수정 명령 생성
            foreach (var (oldRel, newRel) in changes.ModifiedRelationships)
            {
                commands.Add(new DatabaseCommand
                {
                    Query = $"ALTER TABLE [{oldRel.SourceTable}] DROP CONSTRAINT [{oldRel.Name}]"
                });
                commands.Add(GenerateCreateRelationshipCommand(newRel));
            }

            return commands;
        }

        private DatabaseCommand GenerateCreateTableCommand(TableModel table)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE [{table.Name}] (");


           

            // 컬럼 정의
            var columnDefinitions = new List<string>();
            foreach (var column in table.Columns)
            {
                var columnDef = new StringBuilder();
                columnDef.Append($"    [{column.Name}] {GetSqlDataType(column)}");

                if (!column.IsNullable)
                    columnDef.Append(" NOT NULL");

                if (!string.IsNullOrEmpty(column.DefaultValue))
                    columnDef.Append($" DEFAULT {column.DefaultValue}");

                columnDefinitions.Add(columnDef.ToString());
            }

            // 기본키 정의
            var primaryKeys = table.Columns.Where(c => c.IsPrimaryKey).ToList();
            if (primaryKeys.Any())
            {
                columnDefinitions.Add(
                    $"    CONSTRAINT [PK_{table.Name}] PRIMARY KEY CLUSTERED " +
                    $"([{string.Join("], [", primaryKeys.Select(pk => pk.Name))}])"
                );
            }

            sb.AppendLine(string.Join(",\n", columnDefinitions));
            sb.AppendLine(")");

            return new DatabaseCommand
            {
                Query = sb.ToString()
            };
        }

        private DatabaseCommand GenerateCreateRelationshipCommand(RelationshipModel relationship)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"ALTER TABLE [{relationship.SourceTable}]");
            sb.AppendLine($"ADD CONSTRAINT [{relationship.Name}]");
            sb.AppendLine($"FOREIGN KEY ([{GetForeignKeyColumnName(relationship)}])");
            sb.AppendLine($"REFERENCES [{relationship.TargetTable}] ([{GetPrimaryKeyColumnName(relationship)}])");

            if (relationship.OnDelete != ReferentialAction.NoAction)
                sb.AppendLine($"ON DELETE {relationship.OnDelete.ToString().ToUpper()}");

            if (relationship.OnUpdate != ReferentialAction.NoAction)
                sb.AppendLine($"ON UPDATE {relationship.OnUpdate.ToString().ToUpper()}");

            return new DatabaseCommand
            {
                Query = sb.ToString()
            };
        }

        private IEnumerable<DatabaseCommand> GenerateAlterTableCommands(TableChanges changes)
        {
            var commands = new List<DatabaseCommand>();

            // 삭제된 컬럼
            foreach (var column in changes.RemovedColumns)
            {
                commands.Add(new DatabaseCommand
                {
                    Query = $"ALTER TABLE [{changes.TableName}] DROP COLUMN [{column.Name}]"
                });
            }

            // 추가된 컬럼
            foreach (var column in changes.AddedColumns)
            {
                var sb = new StringBuilder();
                sb.Append($"ALTER TABLE [{changes.TableName}] ADD [{column.Name}] {GetSqlDataType(column)}");

                if (!column.IsNullable)
                    sb.Append(" NOT NULL");

                if (!string.IsNullOrEmpty(column.DefaultValue))
                    sb.Append($" DEFAULT {column.DefaultValue}");

                commands.Add(new DatabaseCommand { Query = sb.ToString() });
            }

            // 수정된 컬럼
            foreach (var (oldColumn, newColumn) in changes.ModifiedColumns)
            {
                var sb = new StringBuilder();
                sb.Append($"ALTER TABLE [{changes.TableName}] ALTER COLUMN [{newColumn.Name}] {GetSqlDataType(newColumn)}");

                if (!newColumn.IsNullable)
                    sb.Append(" NOT NULL");

                commands.Add(new DatabaseCommand { Query = sb.ToString() });

                // 기본값 변경
                if (oldColumn.DefaultValue != newColumn.DefaultValue)
                {
                    if (!string.IsNullOrEmpty(oldColumn.DefaultValue))
                    {
                        commands.Add(new DatabaseCommand
                        {
                            Query = $"ALTER TABLE [{changes.TableName}] DROP CONSTRAINT [DF_{changes.TableName}_{oldColumn.Name}]"
                        });
                    }

                    if (!string.IsNullOrEmpty(newColumn.DefaultValue))
                    {
                        commands.Add(new DatabaseCommand
                        {
                            Query = $"ALTER TABLE [{changes.TableName}] ADD CONSTRAINT [DF_{changes.TableName}_{newColumn.Name}] " +
                                   $"DEFAULT {newColumn.DefaultValue} FOR [{newColumn.Name}]"
                        });
                    }
                }
            }

            return commands;
        }

        private string GetSqlDataType(ColumnModel column)
        {
            return column.DataType switch
            {
                DataType.Int => "INT",
                DataType.Bigint => "BIGINT",
                DataType.Varchar when column.Length.HasValue => $"VARCHAR({(column.Length == -1 ? "MAX" : column.Length.ToString())})",
                DataType.Varchar => "VARCHAR(MAX)",
                DataType.Nvarchar when column.Length.HasValue => $"NVARCHAR({(column.Length == -1 ? "MAX" : column.Length.ToString())})",
                DataType.Nvarchar => "NVARCHAR(MAX)",
                DataType.Decimal when column.Precision.HasValue && column.Scale.HasValue => $"DECIMAL({column.Precision},{column.Scale})",
                DataType.Decimal => "DECIMAL(18,2)",
                DataType.Datetime => "DATETIME2",
                DataType.Bool => "BIT",
                _ => column.DataType.ToString()
            };
        }

        private DataType ParseDataType(string sqlType)
        {
            return sqlType.ToLower() switch
            {
                "int" => DataType.Int,
                "bigint" => DataType.Bigint,
                "varchar" => DataType.Varchar,
                "nvarchar" => DataType.Nvarchar,
                "decimal" => DataType.Decimal,
                "datetime2" => DataType.Datetime,
                "bit" => DataType.Bool,
                _ => throw new ArgumentException($"Unsupported SQL type: {sqlType}")
            };
        }

        private ReferentialAction ParseReferentialAction(string action)
        {
            return action?.ToUpper() switch
            {
                "CASCADE" => ReferentialAction.Cascade,
                "SET NULL" => ReferentialAction.SetNull,
                "SET DEFAULT" => ReferentialAction.SetDefault,
                "RESTRICT" => ReferentialAction.Restrict,
                _ => ReferentialAction.NoAction
            };
        }

        private bool RelationshipsAreEqual(RelationshipModel rel1, RelationshipModel rel2)
        {
            return rel1.RelationType == rel2.RelationType &&
                   rel1.OnDelete == rel2.OnDelete &&
                   rel1.OnUpdate == rel2.OnUpdate;
        }

        private string GetForeignKeyColumnName(RelationshipModel relationship)
        {
            return $"{relationship.TargetTable}Id";
        }

        private string GetPrimaryKeyColumnName(RelationshipModel relationship)
        {
            return "Id";
        }

        private async Task<bool> ValidatePermissions()
        {
            var requiredPermissions = new[]
            {
                DatabasePermissions.Create,
                DatabasePermissions.Alter,
                DatabasePermissions.Drop
            };

            foreach (var permission in requiredPermissions)
            {
                if (!await _securityManager.ValidatePermissions(permission))
                {
                    return false;
                }
            }

            return true;
        }

        private async Task CreateSchemaBackup()
        {
            var backupPath = $"Schema_Backup_{DateTime.Now:yyyyMMddHHmmss}.bak";
            await _databaseManager.BackupDatabase(backupPath);
        }

        private async Task<DatabaseOperationResult> ExecuteSchemaChanges(
            IEnumerable<DatabaseCommand> commands, SchemaUpdateOptions options)
        {
            if (options.UseTransaction)
            {
                return await _databaseManager.ExecuteTransaction(commands);
            }

            foreach (var command in commands)
            {
                var result = await _databaseManager.ExecuteNonQuery(command.Query, command.Parameters);
                if (!result.Success)
                {
                    return result;
                }
            }

            return new DatabaseOperationResult { Success = true };
        }
    }

    public class SchemaUpdateOptions
    {
        public bool UseTransaction { get; set; } = true;
        public bool AllowTableDrop { get; set; } = false;
        public bool CreateBackupBeforeChanges { get; set; } = true;
        public bool ValidateBeforeChanges { get; set; } = true;
        public bool IgnoreCase { get; set; } = true;
        public List<string> ExcludedTables { get; set; } = new();
        public Dictionary<string, string> CustomDataTypeMap { get; set; } = new();
    }

    public class SchemaChanges
    {
        public List<TableModel> AddedTables { get; set; } = new();
        public List<TableModel> RemovedTables { get; set; } = new();
        public List<TableChanges> ModifiedTables { get; set; } = new();
        public List<RelationshipModel> AddedRelationships { get; set; } = new();
        public List<RelationshipModel> RemovedRelationships { get; set; } = new();
        public List<(RelationshipModel Old, RelationshipModel New)> ModifiedRelationships { get; set; } = new();

        public bool HasChanges =>
            AddedTables.Any() || RemovedTables.Any() || ModifiedTables.Any() ||
            AddedRelationships.Any() || RemovedRelationships.Any() || ModifiedRelationships.Any();
    }

    public class TableChanges
    {
        public string TableName { get; set; }
        public List<ColumnModel> AddedColumns { get; set; } = new();
        public List<ColumnModel> RemovedColumns { get; set; } = new();
        public List<(ColumnModel Old, ColumnModel New)> ModifiedColumns { get; set; } = new();
        public List<IndexModel> AddedIndexes { get; set; } = new();
        public List<IndexModel> RemovedIndexes { get; set; } = new();
        public List<(IndexModel Old, IndexModel New)> ModifiedIndexes { get; set; } = new();

        public bool HasChanges =>
            AddedColumns.Any() || RemovedColumns.Any() || ModifiedColumns.Any() ||
            AddedIndexes.Any() || RemovedIndexes.Any() || ModifiedIndexes.Any();
    }
}