using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using PC1MAINAITradingSystem.Core.ERD.Models;

namespace PC1MAINAITradingSystem.Core.Database
{
    public partial class DatabaseSchemaManager
    {
        public async Task<ERDModel> ExtractCurrentSchema()
        {
            try
            {
                await _logger.LogInfo("Starting current schema extraction");

                var model = new ERDModel
                {
                    Version = "1.0",
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };

                // 테이블 정보 추출
                model.Tables = await ExtractTables();

                // 관계 정보 추출
                model.Relationships = await ExtractRelationships();

                await _logger.LogInfo($"Schema extraction completed. Found {model.Tables.Count} tables and {model.Relationships.Count} relationships");
                return model;
            }
            catch (Exception ex)
            {
                await _logger.LogError("Schema extraction failed", ex);
                throw;
            }
        }

        private async Task<List<TableModel>> ExtractTables()
        {
            var tables = new List<TableModel>();

            var query = @"
                SELECT 
                    t.name AS TableName,
                    SCHEMA_NAME(t.schema_id) AS SchemaName,
                    p.value AS TableDescription
                FROM sys.tables t
                LEFT JOIN sys.extended_properties p ON
                    p.major_id = t.object_id AND
                    p.minor_id = 0 AND
                    p.name = 'MS_Description'
                ORDER BY TableName";

            var result = await _databaseManager.ExecuteQuery(query);
            if (!result.Success) throw new Exception("Failed to extract tables");

            foreach (var row in result.Data)
            {
                var table = new TableModel
                {
                    Name = row["TableName"].ToString(),
                    Description = row["TableDescription"]?.ToString(),
                    Columns = await ExtractColumns(row["TableName"].ToString()),
                    Indexes = await ExtractIndexes(row["TableName"].ToString())
                };

                tables.Add(table);
            }

            return tables;
        }

        private async Task<List<ColumnModel>> ExtractColumns(string tableName)
        {
            // ... ExtractColumns 메서드 구현 ...
        }

        private async Task<List<IndexModel>> ExtractIndexes(string tableName)
        {
            // ... ExtractIndexes 메서드 구현 ...
        }

        private async Task<List<RelationshipModel>> ExtractRelationships()
        {
            // ... ExtractRelationships 메서드 구현 ...
        }
    }
}