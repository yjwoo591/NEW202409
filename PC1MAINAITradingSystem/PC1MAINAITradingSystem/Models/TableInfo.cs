using System.Collections.Generic;

namespace PC1MAINAITradingSystem.Models
{
    public class TableInfo
    {
        public string Name { get; set; }
        public List<ColumnInfo> Columns { get; set; }
    }

    public class ColumnInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
}