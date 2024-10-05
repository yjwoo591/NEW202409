using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace ForexAITradingPC1Main.Database
{
    public class DatabaseCreator
    {
        private string connectionString;

        public DatabaseCreator(string server, string username, string password)
        {
            this.connectionString = $"Server={server};User Id={username};Password={password};TrustServerCertificate=True;";
        }

        public async Task CreateDatabaseAsync(string databaseName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string sql = $"IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{databaseName}') " +
                             $"BEGIN " +
                             $"CREATE DATABASE {databaseName};" +
                             $"END;";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task CreateTableAsync(string databaseName, string tableName, List<ColumnDefinition> columns)
        {
            using (SqlConnection connection = new SqlConnection(connectionString + $"Database={databaseName};"))
            {
                await connection.OpenAsync();

                string columnDefinitions = string.Join(", ", columns.ConvertAll(col => $"{col.Name} {col.DataType}{(col.IsPrimaryKey ? " PRIMARY KEY" : "")}{(col.IsNullable ? " NULL" : " NOT NULL")}"));
                string sql = $"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '{tableName}') " +
                             $"BEGIN " +
                             $"CREATE TABLE {tableName} ({columnDefinitions});" +
                             $"END;";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<bool> DatabaseExistsAsync(string databaseName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string sql = $"SELECT COUNT(*) FROM sys.databases WHERE name = '{databaseName}'";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    int count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return count > 0;
                }
            }
        }

        public async Task<bool> TableExistsAsync(string databaseName, string tableName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString + $"Database={databaseName};"))
            {
                await connection.OpenAsync();

                string sql = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    int count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return count > 0;
                }
            }
        }

        public async Task DropTableAsync(string databaseName, string tableName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString + $"Database={databaseName};"))
            {
                await connection.OpenAsync();

                string sql = $"IF EXISTS (SELECT * FROM sys.tables WHERE name = '{tableName}') " +
                             $"BEGIN " +
                             $"DROP TABLE {tableName};" +
                             $"END;";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }

    public class ColumnDefinition
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsNullable { get; set; }

        public ColumnDefinition(string name, string dataType, bool isPrimaryKey = false, bool isNullable = true)
        {
            Name = name;
            DataType = dataType;
            IsPrimaryKey = isPrimaryKey;
            IsNullable = isNullable;
        }
    }
}

/*이 DatabaseCreator.cs 파일은 다음과 같은 주요 특징을 가지고 있습니다:

데이터베이스 생성 기능(CreateDatabaseAsync)
테이블 생성 기능(CreateTableAsync)
데이터베이스 존재 여부 확인 기능(DatabaseExistsAsync)
존재 여부 확인 기능(TableExistsAsync)
테이블 삭제 기능(DropTableAsync)

모든 메서드는 특정 요소로 구성되어 있어 성능을 최적화하고 있습니다.
주요 기능:

CreateDatabaseAsync: 이름의 데이터베이스를 생성합니다.
CreateTableAsync: 데이터베이스에 테이블을 생성합니다. 상대방의 정의는 환영 테이블 구조를 정의합니다.
DatabaseExistsAsync: 지명된 이름의 데이터베이스가 존재하는지 확인합니다.
TableExistsAsync: 지정된 데이터베이스에 특정 테이블이 있음을 확인합니다.
DropTableAsync: 데이터베이스에서 특정 테이블을 삭제합니다.

ColumnDefinition 클래스는 테이블에 따른 속성을 정의하는 데 사용됩니다. 이를 통해 이름, 데이터 유형, 기본 키 여부, NULL 고유 여부를 결정할 수 있습니다.
이 클래스를 사용하여 데이터베이스와 테이블을 프로그래밍하여 생성하고 관리할 수 있습니다.
예를 들어, ERD에서 정의된 테이블 구조를 통해 이 클래스를 통해 실제 데이터베이스에 대해 알아볼 수 있습니다.
*/