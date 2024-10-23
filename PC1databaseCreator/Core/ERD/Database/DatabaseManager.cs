using System;
using System.Data;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Collections.Generic;
using PC1MAINAITradingSystem.Core.ERD.Models;

namespace PC1MAINAITradingSystem.Core.Database
{
    public class DatabaseManager
    {
        private readonly ILogger _logger;
        private readonly SqlConnection _connection;
        private readonly DatabaseConfig _config;
        private readonly ISecurityManager _securityManager;
        private bool _isInitialized;

        public DatabaseManager(
            ILogger logger,
            DatabaseConfig config,
            ISecurityManager securityManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _securityManager = securityManager ?? throw new ArgumentNullException(nameof(securityManager));
            _connection = new SqlConnection();
        }

        public async Task<bool> Initialize()
        {
            try
            {
                if (_isInitialized)
                {
                    await _logger.LogWarning("Database manager is already initialized");
                    return true;
                }

                // 설정 검증
                if (!ValidateConfiguration())
                {
                    return false;
                }

                // 연결 문자열 생성
                var connectionString = BuildConnectionString();
                _connection.ConnectionString = connectionString;

                // 연결 테스트
                await TestConnection();

                _isInitialized = true;
                await _logger.LogInfo("Database manager initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogError("Failed to initialize database manager", ex);
                return false;
            }
        }

        public async Task<DatabaseOperationResult> ExecuteQuery(string query, Dictionary<string, object> parameters = null)
        {
            try
            {
                if (!_isInitialized)
                {
                    throw new InvalidOperationException("Database manager is not initialized");
                }

                await EnsureConnectionOpen();

                using var command = CreateCommand(query, parameters);
                using var reader = await command.ExecuteReaderAsync();

                var result = new DatabaseOperationResult
                {
                    Success = true,
                    Data = await ReadQueryResults(reader)
                };

                return result;
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Query execution failed: {query}", ex);
                return new DatabaseOperationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<DatabaseOperationResult> ExecuteNonQuery(string query, Dictionary<string, object> parameters = null)
        {
            try
            {
                if (!_isInitialized)
                {
                    throw new InvalidOperationException("Database manager is not initialized");
                }

                await EnsureConnectionOpen();

                using var command = CreateCommand(query, parameters);
                var rowsAffected = await command.ExecuteNonQueryAsync();

                return new DatabaseOperationResult
                {
                    Success = true,
                    RowsAffected = rowsAffected
                };
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Non-query execution failed: {query}", ex);
                return new DatabaseOperationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<DatabaseOperationResult> ExecuteTransaction(IEnumerable<DatabaseCommand> commands)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Database manager is not initialized");
            }

            await EnsureConnectionOpen();
            using var transaction = _connection.BeginTransaction();

            try
            {
                foreach (var command in commands)
                {
                    using var sqlCommand = CreateCommand(command.Query, command.Parameters);
                    sqlCommand.Transaction = transaction;
                    await sqlCommand.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                return new DatabaseOperationResult { Success = true };
            }
            catch (Exception ex)
            {
                await _logger.LogError("Transaction execution failed", ex);
                await transaction.RollbackAsync();
                return new DatabaseOperationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<DatabaseOperationResult> BackupDatabase(string backupPath)
        {
            try
            {
                if (!_isInitialized)
                {
                    throw new InvalidOperationException("Database manager is not initialized");
                }

                // 백업 권한 확인
                if (!await _securityManager.ValidatePermissions(DatabasePermissions.Backup))
                {
                    throw new UnauthorizedAccessException("Insufficient permissions for database backup");
                }

                var query = $@"
                    BACKUP DATABASE [{_config.DatabaseName}]
                    TO DISK = @BackupPath
                    WITH FORMAT, 
                         INIT, 
                         NAME = N'Full Database Backup',
                         DESCRIPTION = N'Backup created at {DateTime.Now:yyyy-MM-dd HH:mm:ss}',
                         STATS = 10";

                var parameters = new Dictionary<string, object>
                {
                    { "@BackupPath", backupPath }
                };

                var result = await ExecuteNonQuery(query, parameters);
                if (result.Success)
                {
                    await _logger.LogInfo($"Database backup created successfully at {backupPath}");
                }

                return result;
            }
            catch (Exception ex)
            {
                await _logger.LogError("Database backup failed", ex);
                return new DatabaseOperationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<DatabaseOperationResult> RestoreDatabase(string backupPath)
        {
            try
            {
                if (!_isInitialized)
                {
                    throw new InvalidOperationException("Database manager is not initialized");
                }

                // 복원 권한 확인
                if (!await _securityManager.ValidatePermissions(DatabasePermissions.Restore))
                {
                    throw new UnauthorizedAccessException("Insufficient permissions for database restore");
                }

                // 현재 연결 종료
                await _connection.CloseAsync();

                // master 데이터베이스로 연결
                var masterConnection = new SqlConnection(BuildConnectionString("master"));
                await masterConnection.OpenAsync();

                try
                {
                    // 단일 사용자 모드로 전환
                    var singleUserQuery = $@"
                        ALTER DATABASE [{_config.DatabaseName}] 
                        SET SINGLE_USER 
                        WITH ROLLBACK IMMEDIATE";

                    using (var command = masterConnection.CreateCommand())
                    {
                        command.CommandText = singleUserQuery;
                        await command.ExecuteNonQueryAsync();
                    }

                    // 데이터베이스 복원
                    var restoreQuery = $@"
                        RESTORE DATABASE [{_config.DatabaseName}]
                        FROM DISK = @BackupPath
                        WITH REPLACE,
                             STATS = 10";

                    using (var command = masterConnection.CreateCommand())
                    {
                        command.CommandText = restoreQuery;
                        command.Parameters.AddWithValue("@BackupPath", backupPath);
                        await command.ExecuteNonQueryAsync();
                    }

                    // 다중 사용자 모드로 복원
                    var multiUserQuery = $@"
                        ALTER DATABASE [{_config.DatabaseName}]
                        SET MULTI_USER";

                    using (var command = masterConnection.CreateCommand())
                    {
                        command.CommandText = multiUserQuery;
                        await command.ExecuteNonQueryAsync();
                    }

                    await _logger.LogInfo($"Database restored successfully from {backupPath}");
                    return new DatabaseOperationResult { Success = true };
                }
                finally
                {
                    await masterConnection.CloseAsync();
                    await EnsureConnectionOpen(); // 원래 데이터베이스로 재연결
                }
            }
            catch (Exception ex)
            {
                await _logger.LogError("Database restore failed", ex);
                return new DatabaseOperationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        private bool ValidateConfiguration()
        {
            if (string.IsNullOrWhiteSpace(_config.Server))
            {
                _logger.LogError("Database server is not specified").Wait();
                return false;
            }

            if (string.IsNullOrWhiteSpace(_config.DatabaseName))
            {
                _logger.LogError("Database name is not specified").Wait();
                return false;
            }

            return true;
        }

        private string BuildConnectionString(string database = null)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = _config.Server,
                InitialCatalog = database ?? _config.DatabaseName,
                IntegratedSecurity = _config.IntegratedSecurity
            };

            if (!_config.IntegratedSecurity)
            {
                builder.UserID = _config.Username;
                builder.Password = _config.Password;
            }

            builder.ApplicationName = _config.ApplicationName;
            builder.ConnectTimeout = _config.ConnectionTimeout;
            builder.MultipleActiveResultSets = true;

            return builder.ToString();
        }

        private async Task TestConnection()
        {
            try
            {
                await _connection.OpenAsync();
                await _connection.CloseAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Database connection test failed", ex);
            }
        }

        private async Task EnsureConnectionOpen()
        {
            if (_connection.State != ConnectionState.Open)
            {
                await _connection.OpenAsync();
            }
        }

        private SqlCommand CreateCommand(string query, Dictionary<string, object> parameters)
        {
            var command = _connection.CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                }
            }

            return command;
        }

        private async Task<List<Dictionary<string, object>>> ReadQueryResults(SqlDataReader reader)
        {
            var results = new List<Dictionary<string, object>>();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                results.Add(row);
            }

            return results;
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection != null)
            {
                await _connection.DisposeAsync();
            }
        }
    }

    public class DatabaseCommand
    {
        public string Query { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }

    public class DatabaseOperationResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public int RowsAffected { get; set; }
        public List<Dictionary<string, object>> Data { get; set; }
    }

    public class DatabaseConfig
    {
        public string Server { get; set; }
        public string DatabaseName { get; set; }
        public bool IntegratedSecurity { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ApplicationName { get; set; } = "ERD Manager";
        public int ConnectionTimeout { get; set; } = 30;
        public bool EnableRetry { get; set; } = true;
        public int MaxRetryAttempts { get; set; } = 3;
        public int RetryInterval { get; set; } = 1000;
        public bool EnablePooling { get; set; } = true;
        public int MinPoolSize { get; set; } = 1;
        public int MaxPoolSize { get; set; } = 100;
    }

    public enum DatabasePermissions
    {
        Read,
        Write,
        Execute,
        Backup,
        Restore,
        Create,
        Alter,
        Drop,
        Admin
    }
}