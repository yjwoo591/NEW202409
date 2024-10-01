using System.Data.SqlClient;
using System.IO;
using Newtonsoft.Json;
using ForexAITradingPC2.Models;

namespace ForexAITradingPC2.Utils
{
    public class DatabaseManager
    {
        private SqlConnection sqlConnection;
        private DatabaseConfig dbConfig;
        private const string CONFIG_PATH = "dbconfig.json";

        public DatabaseManager()
        {
            LoadConfig();
        }

        public void LoadConfig()
        {
            if (File.Exists(CONFIG_PATH))
            {
                string json = File.ReadAllText(CONFIG_PATH);
                dbConfig = JsonConvert.DeserializeObject<DatabaseConfig>(json);
                InitializeDatabase();
            }
        }


        public bool HasValidConfig()
        {
            return dbConfig != null &&
                   !string.IsNullOrWhiteSpace(dbConfig.ServerIP) &&
                   !string.IsNullOrWhiteSpace(dbConfig.UserID) &&
                   !string.IsNullOrWhiteSpace(dbConfig.Password);
        }



        public void SaveConfig(DatabaseConfig config)
        {
            dbConfig = config;
            string json = JsonConvert.SerializeObject(dbConfig);
            File.WriteAllText(CONFIG_PATH, json);
        }

        public DatabaseConfig GetConfig()
        {
            return dbConfig;
        }

        public void InitializeDatabase()
        {
            if (dbConfig != null)
            {
                string connectionString = $"Server={dbConfig.ServerIP};Database={dbConfig.DatabaseName};User Id={dbConfig.UserID};Password={dbConfig.Password};";
                sqlConnection = new SqlConnection(connectionString);
            }
        }

        // 데이터베이스 연결 테스트 메서드
        public bool TestConnection()
        {
            try
            {
                sqlConnection.Open();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (sqlConnection.State == System.Data.ConnectionState.Open)
                    sqlConnection.Close();
            }
        }

        // 여기에 추가적인 데이터베이스 관련 메서드들을 구현할 수 있습니다.
        // 예: ExecuteQuery, ExecuteNonQuery 등
    }
}