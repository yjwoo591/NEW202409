using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PC1MAINAITradingSystem.Utils
{
    public class SqlHelper
    {
        private readonly SqlConnection _connection;
        private readonly Logger _logger;

        public SqlHelper(SqlConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _logger = Logger.Instance;
        }

        public async Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object> parameters = null)
        {
            try
            {
                using var command = CreateCommand(query, parameters);
                await EnsureConnectionOpenAsync();
                return await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                await _logger.Error($"ExecuteNonQuery failed. Query: {query}", ex);
                throw;
            }
        }

        public async Task<object> ExecuteScalarAsync(string query, Dictionary<string, object> parameters = null)
        {
            try
            {
                using var command = CreateCommand(query, parameters);
                await EnsureConnectionOpenAsync();
                return await command.ExecuteScalarAsync();
            }
            catch (Exception ex)
            {
                await _logger.Error($"ExecuteScalar failed. Query: {query}", ex);
                throw;
            }
        }

        public async Task<DataTable> ExecuteReaderAsync(string query, Dictionary<string, object> parameters = null)
        {
            try
            {
                using var command = CreateCommand(query, parameters);
                await EnsureConnectionOpenAsync();

                using var reader = await command.ExecuteReaderAsync();
                var dataTable = new DataTable();
                dataTable.Load(reader);
                return dataTable;
            }
            catch (Exception ex)
            {
                await _logger.Error($"ExecuteReader failed. Query: {query}", ex);
                throw;
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                await EnsureConnectionOpenAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _logger.Error("Database connection test failed", ex);
                return false;
            }
        }

        private SqlCommand CreateCommand(string query, Dictionary<string, object> parameters)
        {
            var command = _connection.CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
            }

            return command;
        }

        private async Task EnsureConnectionOpenAsync()
        {
            if (_connection.State != ConnectionState.Open)
            {
                await _connection.OpenAsync();
            }
        }

        public async Task BackupDatabaseAsync(string backupPath)
        {
            try
            {
                string databaseName = _connection.Database;
                string query = $@"BACKUP DATABASE [{databaseName}] 
                                TO DISK = @BackupPath 
                                WITH FORMAT, INIT, NAME = @DatabaseName,
                                SKIP, NOREWIND, NOUNLOAD, STATS = 10";

                var parameters = new Dictionary<string, object>
                {
                    { "@BackupPath", backupPath },
                    { "@DatabaseName", databaseName }
                };

                await ExecuteNonQueryAsync(query, parameters);
                await _logger.Info($"Database backup completed successfully: {backupPath}");
            }
            catch (Exception ex)
            {
                await _logger.Error("Database backup failed", ex);
                throw;
            }
        }

        public async Task RestoreDatabaseAsync(string backupPath)
        {
            try
            {
                string databaseName = _connection.Database;

                // 기존 연결 종료
                var connectionString = _connection.ConnectionString;
                await _connection.CloseAsync();

                // master 데이터베이스로 연결
                using var masterConnection = new SqlConnection(connectionString.Replace(databaseName, "master"));
                await masterConnection.OpenAsync();

                // 단일 사용자 모드로 전환
                string singleUserQuery = $"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
                using (var command = masterConnection.CreateCommand())
                {
                    command.CommandText = singleUserQuery;
                    await command.ExecuteNonQueryAsync();
                }

                // 데이터베이스 복원
                string restoreQuery = $@"RESTORE DATABASE [{databaseName}] 
                                       FROM DISK = @BackupPath 
                                       WITH REPLACE, STATS = 10";

                using (var command = masterConnection.CreateCommand())
                {
                    command.CommandText = restoreQuery;
                    command.Parameters.AddWithValue("@BackupPath", backupPath);
                    await command.ExecuteNonQueryAsync();
                }

                // 다중 사용자 모드로 복원
                string multiUserQuery = $"ALTER DATABASE [{databaseName}] SET MULTI_USER";
                using (var command = masterConnection.CreateCommand())
                {
                    command.CommandText = multiUserQuery;
                    await command.ExecuteNonQueryAsync();
                }

                await _logger.Info($"Database restore completed successfully from: {backupPath}");
            }
            catch (Exception ex)
            {
                await _logger.Error("Database restore failed", ex);
                throw;
            }
            finally
            {
                // 원래 데이터베이스로 재연결
                if (_connection.State != ConnectionState.Open)
                {
                    await _connection.OpenAsync();
                }
            }
        }
    }
}