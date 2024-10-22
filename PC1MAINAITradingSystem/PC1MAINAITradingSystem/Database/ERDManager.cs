using System;
using System.IO;
using PC1MAINAITradingSystem.Models;

namespace PC1MAINAITradingSystem.Database
{
    public class ERDManager
    {
        private DatabaseManager _dbManager;
        private string _currentERD;

        public ERDManager(DatabaseManager dbManager)
        {
            _dbManager = dbManager;
        }

        public bool ReadERD(string filePath)
        {
            try
            {
                _currentERD = File.ReadAllText(filePath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void SaveERD(string filePath)
        {
            File.WriteAllText(filePath, _currentERD);
        }

        public string GetCurrentERD()
        {
            return _currentERD;
        }

        public void GenerateERD()
        {
            // 실제 ERD 생성 로직을 구현
            // 이 예제에서는 간단히 테이블 목록을 문자열로 만듦
            var tables = _dbManager.GetTables();
            _currentERD = string.Join("\n", tables);
        }
    }
}