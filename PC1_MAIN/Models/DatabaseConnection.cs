
using System;

namespace ForexAITradingSystem.Models
{
    public class DatabaseConnection
    {
        public string ServerIP { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string DatabaseName { get; set; } = "ForexAITrading"; // 기본 데이터베이스 이름
        public int Port { get; set; } = 1433; // SQL Server 기본 포트
        public DateTime LastConnected { get; set; }

        public string GetConnectionString()
        {
            return $"Server={ServerIP},{Port};Database={DatabaseName};User Id={Username};Password={Password};";
        }

        public override string ToString()
        {
            return $"Server: {ServerIP}, Database: {DatabaseName}, User: {Username}, Last Connected: {LastConnected}";
        }

        public DatabaseConnection Clone()
        {
            return new DatabaseConnection
            {
                ServerIP = this.ServerIP,
                Username = this.Username,
                Password = this.Password,
                DatabaseName = this.DatabaseName,
                Port = this.Port,
                LastConnected = this.LastConnected
            };
        }
    }
}