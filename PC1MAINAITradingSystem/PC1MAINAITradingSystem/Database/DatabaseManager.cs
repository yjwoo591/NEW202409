using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace PC1MAINAITradingSystem.Database
{
    public class DatabaseManager
    {
        private Dictionary<string, string> _connectionStrings;
        private CacheManager _cacheManager;
        private QueryOptimizer _queryOptimizer;
        private SqlConnection _currentConnection;
        private string _currentDatabase;

        public bool IsConnected => _currentConnection != null && _currentConnection.State == ConnectionState.Open;

        public DatabaseManager()
        {
            _connectionStrings = new Dictionary<string, string>();
            _cacheManager = new CacheManager();
            _queryOptimizer = new QueryOptimizer();
        }

        public void ConnectToDatabase(string dbType, string connectionString)
        {
            if (!connectionString.Contains("TrustServerCertificate=True"))
            {
                connectionString += ";TrustServerCertificate=True;";
            }

            _connectionStrings[dbType] = connectionString;
            if (_currentConnection != null)
            {
                _currentConnection.Close();
            }
            _currentConnection = new SqlConnection(connectionString);
            _currentConnection.Open();
            _currentDatabase = _currentConnection.Database;
        }

        public void Disconnect()
        {
            if (_currentConnection != null)
            {
                _currentConnection.Close();
                _currentConnection = null;
            }
            _currentDatabase = null;
        }


        public List<string> GetDatabases()
        {
            List<string> databases = new List<string>();
            if (IsConnected)
            {
                try
                {
                    using (var command = _connection.CreateCommand())
                    {
                        command.CommandText = "SELECT name FROM sys.databases WHERE database_id > 4;";
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                databases.Add(reader.GetString(0));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 로그 기록 또는 예외 처리
                    Console.WriteLine($"Error getting databases: {ex.Message}");
                }
            }
            return databases;
        }

        public bool SelectDatabase(string databaseName)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Not connected to a database server.");
            }

            try
            {
                _currentConnection.ChangeDatabase(databaseName);
                _currentDatabase = databaseName;
                return true;
            }
            catch (SqlException)
            {
                return false;
            }
        }

        public DataTable ExecuteQuery(string dbType, string query)
        {
            query = _queryOptimizer.OptimizeQuery(query);
            return _cacheManager.GetOrSet($"{dbType}:{query}", () =>
            {
                using (var connection = GetConnection(dbType))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        var dataTable = new DataTable();
                        new SqlDataAdapter(command).Fill(dataTable);
                        return dataTable;
                    }
                }
            });
        }

        public int ExecuteNonQuery(string dbType, string query)
        {
            query = _queryOptimizer.OptimizeQuery(query);
            using (var connection = GetConnection(dbType))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    return command.ExecuteNonQuery();
                }
            }
        }

        private SqlConnection GetConnection(string dbType)
        {
            if (!_connectionStrings.ContainsKey(dbType))
            {
                throw new ArgumentException($"Database type {dbType} not configured.");
            }
            return new SqlConnection(_connectionStrings[dbType]);
        }
    }
}