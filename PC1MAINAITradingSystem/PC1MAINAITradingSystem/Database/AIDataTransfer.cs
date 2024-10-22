using System;
using System.Collections.Generic;

namespace PC1MAINAITradingSystem.Data
{
    public class AIDataTransfer
    {
        public string SourceDatabase { get; set; }
        public string TargetDatabase { get; set; }
        public DateTime TransferTime { get; set; }
        public List<string> Tables { get; set; }

        public AIDataTransfer()
        {
            Tables = new List<string>();
            TransferTime = DateTime.Now;
        }

        public void AddTable(string tableName)
        {
            if (!Tables.Contains(tableName))
            {
                Tables.Add(tableName);
            }
        }

        public void RemoveTable(string tableName)
        {
            Tables.Remove(tableName);
        }

        public void ClearTables()
        {
            Tables.Clear();
        }
    }
}