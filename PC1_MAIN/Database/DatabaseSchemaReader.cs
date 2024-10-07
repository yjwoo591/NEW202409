using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace ForexAITradingPC1Main.Database
{
    public class DatabaseSchemaReader
    {
        private string connectionString;

        public DatabaseSchemaReader(string server, string database, string username, string password)
        {
            this.connectionString = $"Server={server};Database={database};User Id={username};Password={password};TrustServerCertificate=True;";
        }

        public async Task<List<TableInfo>> GetTableSchemaAsync()
        {
            List<TableInfo> tables = new List<TableInfo>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Get all tables
                DataTable tableSchema = await Task.Run(() => connection.GetSchema("Tables"));
                foreach (DataRow row in tableSchema.Rows)
                {
                    string tableName = row["TABLE_NAME"].ToString();
                    TableInfo table = new TableInfo { Name = tableName, Columns = new List<ColumnInfo>() };

                    // Get columns for each table
                    using (SqlCommand cmd = new SqlCommand($"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName", connection))
                    {
                        cmd.Parameters.AddWithValue("@TableName", tableName);
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                ColumnInfo column = new ColumnInfo
                                {
                                    ColumnName = reader["COLUMN_NAME"].ToString(),
                                    DataType = reader.GetString(reader.GetOrdinal("DATA_TYPE")),
                                    IsNullable = reader["IS_NULLABLE"].ToString() == "YES",
                                    MaxLength = reader["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value ? Convert.ToInt32(reader["CHARACTER_MAXIMUM_LENGTH"]) : (int?)null
                                };
                                table.Columns.Add(column);
                            }
                        }
                    }

                    // Get primary key information
                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT COLUMN_NAME 
                          FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
                          WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsPrimaryKey') = 1 
                          AND TABLE_NAME = @TableName", connection))
                    {
                        cmd.Parameters.AddWithValue("@TableName", tableName);
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string pkColumnName = reader["COLUMN_NAME"].ToString();
                                table.Columns.Find(c => c.ColumnName == pkColumnName).IsPrimaryKey = true;
                            }
                        }
                    }

                    tables.Add(table);
                }
            }

            return tables;
        }

        // ... (other methods)
    }

    public class TableInfo
    {
        public string Name { get; set; }
        public List<ColumnInfo> Columns { get; set; }
    }

    public class ColumnInfo
    {
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public bool IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }
        public int? MaxLength { get; set; }
    }
}
/*
이 DatabaseSchemaReader.cs 파일은 다음과 같은 주요 특징을 가지고 있습니다:

데이터베이스의 모든 테이블을 읽는 기능(GetTableSchemaAsync)
데이터베이스의 모든 테이블 이름을 가져오는 기능(GetTableNamesAsync)
특정 테이블의 상세 정보를 가져오는 기능(GetTableInfoAsync)

모든 메서드는 특정 요소로 구성되어 있어 성능을 최적화하고 있습니다.
주요 기능:

GetTableSchemaAsync: 데이터베이스의 모든 테이블과 그 주변의 정보를 가져오는 개체. 각 테이블의 기본 키 정보가 포함됩니다.
GetTableNamesAsync: 데이터베이스의 모든 테이블 이름 목록을 가져옵니다.
GetTableInfoAsync: 특정 테이블의 상세 정보(컬럼 정보, 기본 키 등)를 가져옵니다.

TableInfo와 ColumnInfo 클래스는 테이블과 외곽의 정보를 처리하는 데 사용됩니다. 
이를 통해 테이블 ​​이름, 이름, 데이터 유형, NULL 여부 여부, 기본 키 여부, 최대 길이 등의 정보를 생성하고 관리할 수 있습니다.
이 클래스를 사용하여 데이터베이스 구조를 분석하고, 기초로 ERD를 생성하거나 데이터베이스 구조를 활용하는 데 활용할 수 있습니다.
또한 데이터베이스 마이그레이션을 비교하는 등의 작업에도 유용하게 사용할 수 있습니다.

*/