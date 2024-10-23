using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using PC1MAINAITradingSystem.Models.ERD;
using System.ComponentModel.DataAnnotations;

namespace PC1MAINAITradingSystem.Core.ERD.Generator
{
    public class SqlGenerator : BaseERDGenerator
    {
        private const string DEFAULT_SCHEMA = "dbo";

        public SqlGenerator(ILogger logger) : base(logger) { }

        public override async Task<ValidationResult> Validate(ERDModel model)
        {
            var result = await ValidateBasic(model);
            if (!result.IsValid)
            {
                return result;
            }

            foreach (var table in model.Tables)
            {
                // SQL 예약어 충돌 검사
                if (IsSqlReservedWord(table.Name))
                {
                    result.AddError($"Table name '{table.Name}' is a SQL reserved word");
                }

                foreach (var column in table.Columns)
                {
                    if (IsSqlReservedWord(column.Name))
                    {
                        result.AddError($"Column name '{column.Name}' in table '{table.Name}' is a SQL reserved word");
                    }

                    // SQL Server 특정 데이터 타입 제약 검사
                    if (!IsValidSqlServerDataType(column))
                    {
                        result.AddError($"Invalid SQL Server data type configuration for column '{column.Name}' in table '{table.Name}'");
                    }
                }
            }

            return result;
        }

        public override async Task<(string Content, List<string> Warnings)> Generate(ERDModel model, GenerationOptions options)
        {
            try
            {
                var warnings = new List<string>();
                var sb = new StringBuilder();

                // SQL 헤더 생성
                GenerateHeader(sb, model, options);

                // 기본 설정
                GenerateDefaultSettings(sb);

                // 테이블 생성
                foreach (var table in model.Tables)
                {
                    await GenerateTableScript(sb, table, options);
                }

                // 외래 키 제약조건 생성
                foreach (var relationship in model.Relationships)
                {
                    await GenerateForeignKeyScript(sb, relationship, model, options);
                }

                // 인덱스 생성
                foreach (var table in model.Tables)
                {
                    await GenerateIndexScripts(sb, table, options);
                }

                // 기본 데이터 삽입 (있는 경우)
                if (options.CustomSettings.TryGetValue("IncludeDefaultData", out var includeDefaultData)
                    && bool.Parse(includeDefaultData))
                {
                    await GenerateDefaultDataScripts(sb, model, options);
                }

                return (sb.ToString(), warnings);
            }
            catch (Exception ex)
            {
                await Logger.LogError("Failed to generate SQL script", ex);
                throw;
            }
        }

        private void GenerateHeader(StringBuilder sb, ERDModel model, GenerationOptions options)
        {
            if (options.IncludeMetadata)
            {
                sb.AppendLine("/*");
                sb.AppendLine($" * Generated SQL Script");
                sb.AppendLine($" * Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($" * ERD Version: {model.Version}");
                sb.AppendLine($" * Tables: {model.Tables.Count}");
                sb.AppendLine($" * Relationships: {model.Relationships.Count}");
                sb.AppendLine(" */");
                sb.AppendLine();
            }

            sb.AppendLine("SET ANSI_NULLS ON");
            sb.AppendLine("GO");
            sb.AppendLine("SET QUOTED_IDENTIFIER ON");
            sb.AppendLine("GO");
            sb.AppendLine();
        }

        private void GenerateDefaultSettings(StringBuilder sb)
        {
            sb.AppendLine("BEGIN TRY");
            sb.AppendLine("    BEGIN TRANSACTION");
            sb.AppendLine();
        }

        private async Task GenerateTableScript(StringBuilder sb, TableModel table, GenerationOptions options)
        {
            if (options.IncludeComments)
            {
                sb.AppendLine($"/* Table: {table.Name} */");
            }

            sb.AppendLine($"CREATE TABLE [{DEFAULT_SCHEMA}].[{table.Name}] (");

            var columnDefinitions = new List<string>();
            var primaryKeyColumns = new List<string>();

            foreach (var column in table.Columns)
            {
                var columnDef = new StringBuilder();
                columnDef.Append($"    [{column.Name}] {GetSqlServerDataType(column)}");

                if (!column.IsNullable)
                {
                    columnDef.Append(" NOT NULL");
                }

                if (!string.IsNullOrEmpty(column.DefaultValue))
                {
                    columnDef.Append($" CONSTRAINT [DF_{table.Name}_{column.Name}] DEFAULT {GetDefaultValueSql(column)}");
                }

                columnDefinitions.Add(columnDef.ToString());

                if (column.IsPrimaryKey)
                {
                    primaryKeyColumns.Add(column.Name);
                }
            }

            // 기본 키 제약조건 추가
            if (primaryKeyColumns.Any())
            {
                columnDefinitions.Add(
                    $"    CONSTRAINT [PK_{table.Name}] PRIMARY KEY CLUSTERED " +
                    $"([{string.Join("], [", primaryKeyColumns)}])");
            }

            sb.AppendLine(string.Join(",\n", columnDefinitions));
            sb.AppendLine(")");
            sb.AppendLine("GO");
            sb.AppendLine();

            // 테이블 설명 추가
            if (!string.IsNullOrEmpty(table.Description))
            {
                sb.AppendLine($"EXEC sp_addextendedproperty");
                sb.AppendLine($"    @name = N'MS_Description',");
                sb.AppendLine($"    @value = N'{EscapeString(table.Description)}',");
                sb.AppendLine($"    @level0type = N'SCHEMA', @level0name = N'{DEFAULT_SCHEMA}',");
                sb.AppendLine($"    @level1type = N'TABLE', @level1name = N'{table.Name}'");
                sb.AppendLine("GO");
                sb.AppendLine();
            }

            // 컬럼 설명 추가
            foreach (var column in table.Columns.Where(c => !string.IsNullOrEmpty(c.Description)))
            {
                sb.AppendLine($"EXEC sp_addextendedproperty");
                sb.AppendLine($"    @name = N'MS_Description',");
                sb.AppendLine($"    @value = N'{EscapeString(column.Description)}',");
                sb.AppendLine($"    @level0type = N'SCHEMA', @level0name = N'{DEFAULT_SCHEMA}',");
                sb.AppendLine($"    @level1type = N'TABLE', @level1name = N'{table.Name}',");
                sb.AppendLine($"    @level2type = N'COLUMN', @level2name = N'{column.Name}'");
                sb.AppendLine("GO");
                sb.AppendLine();
            }
        }

        private async Task GenerateForeignKeyScript(StringBuilder sb, RelationshipModel relationship, ERDModel model, GenerationOptions options)
        {
            var sourceTable = model.Tables.First(t => t.Name == relationship.SourceTable);
            var targetTable = model.Tables.First(t => t.Name == relationship.TargetTable);
            var foreignKeyColumn = GetForeignKeyColumn(sourceTable, targetTable);

            if (foreignKeyColumn != null)
            {
                if (options.IncludeComments)
                {
                    sb.AppendLine($"/* Foreign Key: {relationship.Name} */");
                }

                sb.AppendLine($"ALTER TABLE [{DEFAULT_SCHEMA}].[{relationship.SourceTable}]");
                sb.AppendLine($"ADD CONSTRAINT [{relationship.Name}]");
                sb.AppendLine($"FOREIGN KEY ([{foreignKeyColumn.Name}])");
                sb.AppendLine($"REFERENCES [{DEFAULT_SCHEMA}].[{relationship.TargetTable}] ([Id])");

                // 참조 무결성 규칙 추가
                if (relationship.OnDelete != ReferentialAction.NoAction)
                {
                    sb.AppendLine($"ON DELETE {GetReferentialActionSql(relationship.OnDelete)}");
                }

                if (relationship.OnUpdate != ReferentialAction.NoAction)
                {
                    sb.AppendLine($"ON UPDATE {GetReferentialActionSql(relationship.OnUpdate)}");
                }

                sb.AppendLine("GO");
                sb.AppendLine();
            }
        }

        private async Task GenerateIndexScripts(StringBuilder sb, TableModel table, GenerationOptions options)
        {
            foreach (var index in table.Indexes)
            {
                if (options.IncludeComments)
                {
                    sb.AppendLine($"/* Index: {index.Name} */");
                }

                sb.Append($"CREATE ");
                if (index.IsUnique) sb.Append("UNIQUE ");
                if (index.IsClustered) sb.Append("CLUSTERED ");
                sb.AppendLine($"INDEX [{index.Name}]");
                sb.AppendLine($"ON [{DEFAULT_SCHEMA}].[{table.Name}]");
                sb.AppendLine($"([{string.Join("], [", index.Columns)}])");
                sb.AppendLine("GO");
                sb.AppendLine();
            }
        }

        private async Task GenerateDefaultDataScripts(StringBuilder sb, ERDModel model, GenerationOptions options)
        {
            // 기본 데이터 삽입 스크립트 생성 (필요한 경우)
            sb.AppendLine("/* Default Data Population */");
            // TODO: 기본 데이터 삽입 구현
            sb.AppendLine();
        }

        private string GetSqlServerDataType(ColumnModel column)
        {
            return column.DataType switch
            {
                DataType.Int => "INT",
                DataType.Bigint => "BIGINT",
                DataType.Varchar when column.Length.HasValue =>
                    $"VARCHAR({(column.Length == -1 ? "MAX" : column.Length.ToString())})",
                DataType.Nvarchar when column.Length.HasValue =>
                    $"NVARCHAR({(column.Length == -1 ? "MAX" : column.Length.ToString())})",
                DataType.Char when column.Length.HasValue => $"CHAR({column.Length})",
                DataType.Nchar when column.Length.HasValue => $"NCHAR({column.Length})",
                DataType.Decimal when column.Precision.HasValue && column.Scale.HasValue =>
                    $"DECIMAL({column.Precision},{column.Scale})",
                DataType.Datetime => "DATETIME2",
                DataType.Bool => "BIT",
                DataType.Binary => "BINARY",
                DataType.Varbinary => "VARBINARY(MAX)",
                _ => column.DataType.ToString().ToUpper()
            };
        }

        private string GetDefaultValueSql(ColumnModel column)
        {
            if (string.IsNullOrEmpty(column.DefaultValue))
                return null;

            return column.DataType switch
            {
                DataType.Varchar or DataType.Nvarchar or DataType.Char or DataType.Nchar =>
                    $"N'{EscapeString(column.DefaultValue)}'",
                DataType.Datetime when column.DefaultValue.ToLower() == "now" => "GETDATE()",
                DataType.Bool => column.DefaultValue.ToLower() == "true" ? "1" : "0",
                _ => column.DefaultValue
            };
        }

        private string GetReferentialActionSql(ReferentialAction action)
        {
            return action switch
            {
                ReferentialAction.Cascade => "CASCADE",
                ReferentialAction.SetNull => "SET NULL",
                ReferentialAction.SetDefault => "SET DEFAULT",
                ReferentialAction.Restrict => "RESTRICT",
                _ => "NO ACTION"
            };
        }

        private ColumnModel GetForeignKeyColumn(TableModel sourceTable, TableModel targetTable)
        {
            return sourceTable.Columns.FirstOrDefault(c =>
                c.IsForeignKey && c.Name.Equals($"{targetTable.Name}Id", StringComparison.OrdinalIgnoreCase));
        }

        private string EscapeString(string input)
        {
            return input?.Replace("'", "''");
        }

        private bool IsSqlReservedWord(string word)
        {
            var reservedWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "ADD", "ALTER", "ALL", "AND", "ANY", "AS", "ASC", "BACKUP", "BEGIN", "BETWEEN",
                "BREAK", "BROWSE", "BULK", "BY", "CASCADE", "CASE", "CHECK", "CHECKPOINT",
                "CLOSE", "CLUSTERED", "COALESCE", "COLLATE", "COLUMN", "COMMIT", "CONSTRAINT",
                "CONTAINS", "CONTINUE", "CREATE", "CROSS", "CURRENT", "CURSOR", "DATABASE",
                "DBCC", "DEALLOCATE", "DECLARE", "DEFAULT", "DELETE", "DENY", "DESC", "DISK",
                "DISTINCT", "DISTRIBUTED", "DOUBLE", "DROP", "DUMP", "ELSE", "END", "ERRLVL",
                "ESCAPE", "EXCEPT", "EXEC", "EXECUTE", "EXISTS", "EXIT", "FETCH", "FILE",
                "FILLFACTOR", "FOR", "FOREIGN", "FREETEXT", "FROM", "FULL", "FUNCTION",
                "GOTO", "GRANT", "GROUP", "HAVING", "HOLDLOCK", "IDENTITY", "IDENTITYCOL",
                "IDENTITY_INSERT", "IF", "IN", "INDEX", "INNER", "INSERT", "INTERSECT", "INTO",
                "IS", "JOIN", "KEY", "KILL", "LEFT", "LIKE", "LINENO", "LOAD", "MERGE",
                "NATIONAL", "NOCHECK", "NONCLUSTERED", "NOT", "NULL", "NULLIF", "OF", "OFF",
                "OFFSETS", "ON", "OPEN", "OPENDATASOURCE", "OPENQUERY", "OPENROWSET",
                "OPENXML", "OPTION", "OR", "ORDER", "OUTER", "OVER", "PERCENT", "PIVOT",
                "PLAN", "PRIMARY", "PRINT", "PROC", "PROCEDURE", "PUBLIC", "RAISERROR",
                "READ", "READTEXT", "RECONFIGURE", "REFERENCES", "REPLICATION", "RESTORE",
                "RESTRICT", "RETURN", "REVERT", "REVOKE", "RIGHT", "ROLLBACK", "ROWCOUNT",
                "ROWGUIDCOL", "RULE", "SAVE", "SCHEMA", "SELECT", "SESSION_USER", "SET",
                "SETUSER", "SHUTDOWN", "SOME", "STATISTICS", "SYSTEM_USER", 
                "TABLE", "TEMPORARY", "TEXT", "THEN", "TIME", "TO", "TOP", "TRAN", "TRANSACTION",
                "TRIGGER", "TRUNCATE", "TSEQUAL", "UNION", "UNIQUE", "UNPIVOT", "UPDATE",
                "UPDATETEXT", "USE", "USER", "VALUES", "VARYING", "VIEW", "WAITFOR", "WHEN",
                "WHERE", "WHILE", "WITH", "WRITETEXT"
            };

            return reservedWords.Contains(word);
        }

        private bool IsValidSqlServerDataType(ColumnModel column)
        {
            try
            {
                switch (column.DataType)
                {
                    case DataType.Varchar:
                    case DataType.Nvarchar:
                    case DataType.Char:
                    case DataType.Nchar:
                        return !column.Length.HasValue || column.Length == -1 ||
                               (column.Length > 0 && column.Length <= 8000);

                    case DataType.Decimal:
                    case DataType.Numeric:
                        return (!column.Precision.HasValue || (column.Precision.Value > 0 && column.Precision.Value <= 38)) &&
                               (!column.Scale.HasValue || (column.Scale.Value >= 0 && column.Scale.Value <= column.Precision.Value));

                    case DataType.Int:
                    case DataType.Bigint:
                    case DataType.Bool:
                    case DataType.Datetime:
                    case DataType.Binary:
                    case DataType.Varbinary:
                        return true;

                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private void AppendTransactionFooter(StringBuilder sb)
        {
            sb.AppendLine("    COMMIT TRANSACTION");
            sb.AppendLine("END TRY");
            sb.AppendLine("BEGIN CATCH");
            sb.AppendLine("    IF @@TRANCOUNT > 0");
            sb.AppendLine("        ROLLBACK TRANSACTION");
            sb.AppendLine();
            sb.AppendLine("    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()");
            sb.AppendLine("    DECLARE @ErrorSeverity INT = ERROR_SEVERITY()");
            sb.AppendLine("    DECLARE @ErrorState INT = ERROR_STATE()");
            sb.AppendLine();
            sb.AppendLine("    RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState)");
            sb.AppendLine("END CATCH");
            sb.AppendLine("GO");
        }

        public class SqlGenerationOptions : GenerationOptions
        {
            public bool IncludeTransactions { get; set; } = true;
            public bool GenerateDropStatements { get; set; } = false;
            public bool IncludeIndexes { get; set; } = true;
            public string SchemaName { get; set; } = "dbo";
            public bool AddWithNocheck { get; set; } = false;
            public bool EnableConstraints { get; set; } = true;
            public List<string> ExcludedTables { get; set; } = new();
            public bool GenerateStatistics { get; set; } = false;
        }

        private async Task<string> GenerateDropScript(ERDModel model, SqlGenerationOptions options)
        {
            var sb = new StringBuilder();

            // 외래 키 제약조건 삭제
            foreach (var relationship in model.Relationships.Reverse())
            {
                sb.AppendLine($"IF OBJECT_ID(N'[{options.SchemaName}].[{relationship.Name}]', 'F') IS NOT NULL");
                sb.AppendLine($"    ALTER TABLE [{options.SchemaName}].[{relationship.SourceTable}]");
                sb.AppendLine($"    DROP CONSTRAINT [{relationship.Name}]");
                sb.AppendLine("GO");
                sb.AppendLine();
            }

            // 테이블 삭제
            foreach (var table in model.Tables.Reverse())
            {
                sb.AppendLine($"IF OBJECT_ID(N'[{options.SchemaName}].[{table.Name}]', 'U') IS NOT NULL");
                sb.AppendLine($"    DROP TABLE [{options.SchemaName}].[{table.Name}]");
                sb.AppendLine("GO");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string GetIndexName(TableModel table, IEnumerable<string> columns, bool isUnique = false)
        {
            var columnList = string.Join("_", columns);
            var prefix = isUnique ? "UX" : "IX";
            return $"{prefix}_{table.Name}_{columnList}";
        }

        private string GetForeignKeyName(string sourceTable, string targetTable)
        {
            return $"FK_{sourceTable}_{targetTable}";
        }

        private string FormatDefaultValue(ColumnModel column)
        {
            if (string.IsNullOrEmpty(column.DefaultValue))
                return "NULL";

            switch (column.DataType)
            {
                case DataType.Varchar:
                case DataType.Nvarchar:
                case DataType.Char:
                case DataType.Nchar:
                    return $"N'{EscapeString(column.DefaultValue)}'";

                case DataType.Datetime:
                    return column.DefaultValue.ToUpper() == "NOW" ? "GETDATE()" :
                           $"'{column.DefaultValue}'";

                case DataType.Bool:
                    return bool.Parse(column.DefaultValue) ? "1" : "0";

                default:
                    return column.DefaultValue;
            }
        }
    }
}