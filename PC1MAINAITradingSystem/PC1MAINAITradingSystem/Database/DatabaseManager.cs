```csharp
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Smo;
using PC1MAINAITradingSystem.Interfaces;
using PC1MAINAITradingSystem.Models;

namespace PC1MAINAITradingSystem.Core.DatabaseManager
{
    public class DatabaseManager : IDatabaseManager
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private SqlConnection _connection;
        private string _currentDatabase;

        public DatabaseManager(ILogger logger, IConfiguration config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public bool Connect(string server, string username, string password)
        {
            try
            {
                var connectionString = BuildConnectionString(server, username, password);
                _connection = new SqlConnection(connectionString);
                _connection.Open();

                _logger.Log($"Successfully connected to server: {server}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to connect to database: {ex.Message}");
                throw new DatabaseConnectionException("Failed to connect to database server", ex);
            }
        }

        public void Disconnect()
        {
            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
                _currentDatabase = null;
                _logger.Log("Database connection closed");
            }
        }

        public bool CreateDatabase(string databaseName, ERDStructure erdStructure)
        {
            try
            {
                EnsureConnected();

                // 데이터베이스 생성
                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = $@"
                        IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{databaseName}')
                        BEGIN
                            CREATE DATABASE [{databaseName}]
                        END";
                    command.ExecuteNonQuery();
                }

                // 새 데이터베이스로 전환
                _connection.ChangeDatabase(databaseName);
                _currentDatabase = databaseName;

                // 테이블 생성
                foreach (var table in erdStructure.Tables)
                {
                    CreateTable(table);
                }

                // 관계 생성
                foreach (var relation in erdStructure.Relations)
                {
                    CreateRelation(relation);
                }

                _logger.Log($"Successfully created database: {databaseName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to create database: {ex.Message}");
                throw new DatabaseCreationException($"Failed to create database {databaseName}", ex);
            }
        }

        public bool ModifyDatabase(string databaseName, List<DatabaseChange> changes)
        {
            try
            {
                EnsureConnected();
                _connection.ChangeDatabase(databaseName);
                _currentDatabase = databaseName;

                foreach (var change in changes)
                {
                    ApplyDatabaseChange(change);
                }

                _logger.Log($"Successfully modified database: {databaseName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to modify database: {ex.Message}");
                throw new DatabaseModificationException($"Failed to modify database {databaseName}", ex);
            }
        }

        public bool BackupDatabase(string databaseName, string backupPath)
        {
            try
            {
                EnsureConnected();

                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = $@"BACKUP DATABASE [{databaseName}] 
                        TO DISK = '{backupPath}' 
                        WITH FORMAT, MEDIANAME = 'Z_SQLServerBackups', 
                        NAME = 'Full Backup of {databaseName}'";
                    command.ExecuteNonQuery();
                }

                _logger.Log($"Successfully backed up database {databaseName} to {backupPath}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to backup database: {ex.Message}");
                throw new DatabaseBackupException($"Failed to backup database {databaseName}", ex);
            }
        }

        public DataTable ExecuteQuery(string query)
        {
            try
            {
                EnsureConnected();

                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = query;
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        var table = new DataTable();
                        adapter.Fill(table);
                        return table;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to execute query: {ex.Message}");
                throw new DatabaseQueryException("Failed to execute query", ex);
            }
        }

        private void CreateTable(TableStructure table)
        {
            var columnDefinitions = new List<string>();
            var primaryKeys = new List<string>();

            foreach (var column in table.Columns)
            {
                var columnDef = $"[{column.Name}] {column.DataType}";

                if (column.Properties.ContainsKey("NotNull") &&
                    column.Properties["NotNull"].Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    columnDef += " NOT NULL";
                }

                if (column.Properties.ContainsKey("PrimaryKey") &&
                    column.Properties["PrimaryKey"].Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    primaryKeys.Add(column.Name);
                }

                columnDefinitions.Add(columnDef);
            }

            if (primaryKeys.Count > 0)
            {
                columnDefinitions.Add(
                    $"CONSTRAINT [PK_{table.Name}] PRIMARY KEY CLUSTERED " +
                    $"([{string.Join("], [", primaryKeys)}])");
            }

            var createTableQuery = $@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{table.Name}]'))
                BEGIN
                    CREATE TABLE [{table.Name}] (
                        {string.Join(",\n        ", columnDefinitions)}
                    )
                END";

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = createTableQuery;
                command.ExecuteNonQuery();
            }

            _logger.Log($"Created table: {table.Name}");
        }

        private void CreateRelation(TableRelation relation)
        {
            var constraintName = $"FK_{relation.SourceTable}_{relation.TargetTable}";
            var createForeignKeyQuery = $@"
                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'{constraintName}'))
                BEGIN
                    ALTER TABLE [{relation.SourceTable}] 
                    ADD CONSTRAINT [{constraintName}] 
                    FOREIGN KEY ([{relation.SourceColumn}]) 
                    REFERENCES [{relation.TargetTable}] ([{relation.TargetColumn}])
                END";

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = createForeignKeyQuery;
                command.ExecuteNonQuery();
            }

            _logger.Log($"Created relation between {relation.SourceTable} and {relation.TargetTable}");
        }

        private void ApplyDatabaseChange(DatabaseChange change)
        {
            switch (change.ChangeType)
            {
                case ChangeType.AddTable:
                    CreateTable(change.TableInfo);
                    break;

                case ChangeType.RemoveTable:
                    DropTable(change.TableName);
                    break;

                case ChangeType.AddColumn:
                    AddColumn(change.TableName, change.ColumnInfo);
                    break;

                case ChangeType.RemoveColumn:
                    RemoveColumn(change.TableName, change.ColumnInfo.Name);
                    break;

                case ChangeType.ModifyColumn:
                    ModifyColumn(change.TableName, change.ColumnInfo);
                    break;

                default:
                    throw new ArgumentException($"Unsupported change type: {change.ChangeType}");
            }
        }

        private void DropTable(string tableName)
        {
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"DROP TABLE IF EXISTS [{tableName}]";
                command.ExecuteNonQuery();
            }
            _logger.Log($"Dropped table: {tableName}");
        }

        private void AddColumn(string tableName, ColumnStructure column)
        {
            var columnDef = $"[{column.Name}] {column.DataType}";
            if (column.Properties.ContainsKey("NotNull") &&
                column.Properties["NotNull"].Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                columnDef += " NOT NULL";
            }

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"ALTER TABLE [{tableName}] ADD {columnDef}";
                command.ExecuteNonQuery();
            }
            _logger.Log($"Added column {column.Name} to table {tableName}");
        }

        private void RemoveColumn(string tableName, string columnName)
        {
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"ALTER TABLE [{tableName}] DROP COLUMN [{columnName}]";
                command.ExecuteNonQuery();
            }
            _logger.Log($"Removed column {columnName} from table {tableName}");
        }

        private void ModifyColumn(string tableName, ColumnStructure column)
        {
            var columnDef = $"[{column.Name}] {column.DataType}";
            if (column.Properties.ContainsKey("NotNull") &&
                column.Properties["NotNull"].Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                columnDef += " NOT NULL";
            }

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"ALTER TABLE [{tableName}] ALTER COLUMN {columnDef}";
                command.ExecuteNonQuery();
            }
            _logger.Log($"Modified column {column.Name} in table {tableName}");
        }

        private void EnsureConnected()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                throw new DatabaseNotConnectedException("Database is not connected");
            }
        }

        private string BuildConnectionString(string server, string username, string password)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = server,
                UserID = username,
                Password = password,
                IntegratedSecurity = string.IsNullOrEmpty(username),
                TrustServerCertificate = true
            };

            return builder.ConnectionString;
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
```