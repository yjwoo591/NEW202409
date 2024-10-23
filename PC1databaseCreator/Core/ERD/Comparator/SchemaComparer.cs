using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using PC1MAINAITradingSystem.Core.ERD.Models;

namespace PC1MAINAITradingSystem.Core.ERD.Comparator
{
    public class SchemaComparer
    {
        private readonly ILogger _logger;

        public SchemaComparer(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SchemaComparisonResult> CompareSchemas(ERDModel oldSchema, ERDModel newSchema)
        {
            try
            {
                await _logger.LogInfo("Starting schema comparison");
                var result = new SchemaComparisonResult
                {
                    CreatedAt = DateTime.UtcNow,
                    OldVersion = oldSchema.Version,
                    NewVersion = newSchema.Version
                };

                // 테이블 비교
                CompareTablesStructure(oldSchema, newSchema, result);

                // 관계 비교
                CompareRelationships(oldSchema, newSchema, result);

                // 인덱스 비교
                CompareIndexes(oldSchema, newSchema, result);

                // 통계 생성
                GenerateStatistics(result);

                await _logger.LogInfo($"Schema comparison completed. Found {result.Changes.Count} changes");
                return result;
            }
            catch (Exception ex)
            {
                await _logger.LogError("Schema comparison failed", ex);
                throw;
            }
        }

        private void CompareTablesStructure(ERDModel oldSchema, ERDModel newSchema, SchemaComparisonResult result)
        {
            // 새로 추가된 테이블
            foreach (var newTable in newSchema.Tables)
            {
                if (!oldSchema.Tables.Any(t => t.Name == newTable.Name))
                {
                    result.AddChange(new SchemaChange
                    {
                        Type = ChangeType.TableAdded,
                        ObjectType = "Table",
                        ObjectName = newTable.Name,
                        Details = $"New table '{newTable.Name}' added with {newTable.Columns.Count} columns"
                    });
                }
            }

            // 삭제된 테이블
            foreach (var oldTable in oldSchema.Tables)
            {
                if (!newSchema.Tables.Any(t => t.Name == oldTable.Name))
                {
                    result.AddChange(new SchemaChange
                    {
                        Type = ChangeType.TableRemoved,
                        ObjectType = "Table",
                        ObjectName = oldTable.Name,
                        Details = $"Table '{oldTable.Name}' removed"
                    });
                }
            }

            // 변경된 테이블
            foreach (var newTable in newSchema.Tables)
            {
                var oldTable = oldSchema.Tables.FirstOrDefault(t => t.Name == newTable.Name);
                if (oldTable != null)
                {
                    CompareTableColumns(oldTable, newTable, result);
                }
            }
        }

        private void CompareTableColumns(TableModel oldTable, TableModel newTable, SchemaComparisonResult result)
        {
            // 새로 추가된 컬럼
            foreach (var newColumn in newTable.Columns)
            {
                if (!oldTable.Columns.Any(c => c.Name == newColumn.Name))
                {
                    result.AddChange(new SchemaChange
                    {
                        Type = ChangeType.ColumnAdded,
                        ObjectType = "Column",
                        ObjectName = $"{newTable.Name}.{newColumn.Name}",
                        Details = $"New column '{newColumn.Name}' added to table '{newTable.Name}'"
                    });
                }
            }

            // 삭제된 컬럼
            foreach (var oldColumn in oldTable.Columns)
            {
                if (!newTable.Columns.Any(c => c.Name == oldColumn.Name))
                {
                    result.AddChange(new SchemaChange
                    {
                        Type = ChangeType.ColumnRemoved,
                        ObjectType = "Column",
                        ObjectName = $"{oldTable.Name}.{oldColumn.Name}",
                        Details = $"Column '{oldColumn.Name}' removed from table '{oldTable.Name}'"
                    });
                }
            }

            // 변경된 컬럼
            foreach (var newColumn in newTable.Columns)
            {
                var oldColumn = oldTable.Columns.FirstOrDefault(c => c.Name == newColumn.Name);
                if (oldColumn != null)
                {
                    CompareColumnProperties(oldColumn, newColumn, newTable.Name, result);
                }
            }
        }

        private void CompareColumnProperties(ColumnModel oldColumn, ColumnModel newColumn,
            string tableName, SchemaComparisonResult result)
        {
            var changes = new List<string>();

            // 데이터 타입 변경
            if (oldColumn.DataType != newColumn.DataType)
            {
                changes.Add($"Data type changed from {oldColumn.DataType} to {newColumn.DataType}");
            }

            // 길이 변경
            if (oldColumn.Length != newColumn.Length)
            {
                changes.Add($"Length changed from {oldColumn.Length} to {newColumn.Length}");
            }

            // Nullable 변경
            if (oldColumn.IsNullable != newColumn.IsNullable)
            {
                changes.Add($"Nullable changed from {oldColumn.IsNullable} to {newColumn.IsNullable}");
            }

            // 기본값 변경
            if (oldColumn.DefaultValue != newColumn.DefaultValue)
            {
                changes.Add($"Default value changed from {oldColumn.DefaultValue} to {newColumn.DefaultValue}");
            }

            // 제약조건 변경
            if (oldColumn.IsPrimaryKey != newColumn.IsPrimaryKey)
            {
                changes.Add($"Primary key constraint {(newColumn.IsPrimaryKey ? "added" : "removed")}");
            }

            if (oldColumn.IsForeignKey != newColumn.IsForeignKey)
            {
                changes.Add($"Foreign key constraint {(newColumn.IsForeignKey ? "added" : "removed")}");
            }

            if (changes.Any())
            {
                result.AddChange(new SchemaChange
                {
                    Type = ChangeType.ColumnModified,
                    ObjectType = "Column",
                    ObjectName = $"{tableName}.{newColumn.Name}",
                    Details = string.Join("; ", changes)
                });
            }
        }

        private void CompareRelationships(ERDModel oldSchema, ERDModel newSchema, SchemaComparisonResult result)
        {
            // 새로 추가된 관계
            foreach (var newRel in newSchema.Relationships)
            {
                if (!oldSchema.Relationships.Any(r =>
                    r.SourceTable == newRel.SourceTable &&
                    r.TargetTable == newRel.TargetTable))
                {
                    result.AddChange(new SchemaChange
                    {
                        Type = ChangeType.RelationshipAdded,
                        ObjectType = "Relationship",
                        ObjectName = $"{newRel.SourceTable}->{newRel.TargetTable}",
                        Details = $"New {newRel.RelationType} relationship added from '{newRel.SourceTable}' to '{newRel.TargetTable}'"
                    });
                }
            }

            // 삭제된 관계
            foreach (var oldRel in oldSchema.Relationships)
            {
                if (!newSchema.Relationships.Any(r =>
                    r.SourceTable == oldRel.SourceTable &&
                    r.TargetTable == oldRel.TargetTable))
                {
                    result.AddChange(new SchemaChange
                    {
                        Type = ChangeType.RelationshipRemoved,
                        ObjectType = "Relationship",
                        ObjectName = $"{oldRel.SourceTable}->{oldRel.TargetTable}",
                        Details = $"Relationship removed between '{oldRel.SourceTable}' and '{oldRel.TargetTable}'"
                    });
                }
            }

            // 변경된 관계
            foreach (var newRel in newSchema.Relationships)
            {
                var oldRel = oldSchema.Relationships.FirstOrDefault(r =>
                    r.SourceTable == newRel.SourceTable &&
                    r.TargetTable == newRel.TargetTable);

                if (oldRel != null && oldRel.RelationType != newRel.RelationType)
                {
                    result.AddChange(new SchemaChange
                    {
                        Type = ChangeType.RelationshipModified,
                        ObjectType = "Relationship",
                        ObjectName = $"{newRel.SourceTable}->{newRel.TargetTable}",
                        Details = $"Relationship type changed from {oldRel.RelationType} to {newRel.RelationType}"
                    });
                }
            }
        }

        private void CompareIndexes(ERDModel oldSchema, ERDModel newSchema, SchemaComparisonResult result)
        {
            foreach (var newTable in newSchema.Tables)
            {
                var oldTable = oldSchema.Tables.FirstOrDefault(t => t.Name == newTable.Name);
                if (oldTable == null) continue;

                // 새로 추가된 인덱스
                foreach (var newIndex in newTable.Indexes)
                {
                    if (!oldTable.Indexes.Any(i => i.Name == newIndex.Name))
                    {
                        result.AddChange(new SchemaChange
                        {
                            Type = ChangeType.IndexAdded,
                            ObjectType = "Index",
                            ObjectName = $"{newTable.Name}.{newIndex.Name}",
                            Details = $"New index '{newIndex.Name}' added to table '{newTable.Name}'"
                        });
                    }
                }

                // 삭제된 인덱스
                foreach (var oldIndex in oldTable.Indexes)
                {
                    if (!newTable.Indexes.Any(i => i.Name == oldIndex.Name))
                    {
                        result.AddChange(new SchemaChange
                        {
                            Type = ChangeType.IndexRemoved,
                            ObjectType = "Index",
                            ObjectName = $"{oldTable.Name}.{oldIndex.Name}",
                            Details = $"Index '{oldIndex.Name}' removed from table '{oldTable.Name}'"
                        });
                    }
                }

                // 변경된 인덱스
                foreach (var newIndex in newTable.Indexes)
                {
                    var oldIndex = oldTable.Indexes.FirstOrDefault(i => i.Name == newIndex.Name);
                    if (oldIndex != null)
                    {
                        CompareIndexProperties(oldIndex, newIndex, newTable.Name, result);
                    }
                }
            }
        }

        private void CompareIndexProperties(IndexModel oldIndex, IndexModel newIndex,
            string tableName, SchemaComparisonResult result)
        {
            var changes = new List<string>();

            // 컬럼 변경
            if (!oldIndex.Columns.SequenceEqual(newIndex.Columns))
            {
                changes.Add($"Columns changed from [{string.Join(", ", oldIndex.Columns)}] to [{string.Join(", ", newIndex.Columns)}]");
            }

            // 유니크 속성 변경
            if (oldIndex.IsUnique != newIndex.IsUnique)
            {
                changes.Add($"Unique property changed from {oldIndex.IsUnique} to {newIndex.IsUnique}");
            }

            // 클러스터드 속성 변경
            if (oldIndex.IsClustered != newIndex.IsClustered)
            {
                changes.Add($"Clustered property changed from {oldIndex.IsClustered} to {newIndex.IsClustered}");
            }

            if (changes.Any())
            {
                result.AddChange(new SchemaChange
                {
                    Type = ChangeType.IndexModified,
                    ObjectType = "Index",
                    ObjectName = $"{tableName}.{newIndex.Name}",
                    Details = string.Join("; ", changes)
                });
            }
        }

        private void GenerateStatistics(SchemaComparisonResult result)
        {
            result.Statistics = new SchemaComparisonStatistics
            {
                TablesAdded = result.Changes.Count(c => c.Type == ChangeType.TableAdded),
                TablesRemoved = result.Changes.Count(c => c.Type == ChangeType.TableRemoved),
                TablesModified = result.Changes.Count(c => c.Type == ChangeType.TableModified),
                ColumnsAdded = result.Changes.Count(c => c.Type == ChangeType.ColumnAdded),
                ColumnsRemoved = result.Changes.Count(c => c.Type == ChangeType.ColumnRemoved),
                ColumnsModified = result.Changes.Count(c => c.Type == ChangeType.ColumnModified),
                RelationshipsAdded = result.Changes.Count(c => c.Type == ChangeType.RelationshipAdded),
                RelationshipsRemoved = result.Changes.Count(c => c.Type == ChangeType.RelationshipRemoved),
                RelationshipsModified = result.Changes.Count(c => c.Type == ChangeType.RelationshipModified),
                IndexesAdded = result.Changes.Count(c => c.Type == ChangeType.IndexAdded),
                IndexesRemoved = result.Changes.Count(c => c.Type == ChangeType.IndexRemoved),
                IndexesModified = result.Changes.Count(c => c.Type == ChangeType.IndexModified)
            };
        }
    }

    public class SchemaComparisonResult
    {
        public DateTime CreatedAt { get; set; }
        public string OldVersion { get; set; }
        public string NewVersion { get; set; }
        public List<SchemaChange> Changes { get; set; } = new();
        public SchemaComparisonStatistics Statistics { get; set; }

        public void AddChange(SchemaChange change)
        {
            change.Timestamp = DateTime.UtcNow;
            Changes.Add(change);
        }
    }

    public class SchemaChange
    {
        public DateTime Timestamp { get; set; }
        public ChangeType Type { get; set; }
        public string ObjectType { get; set; }
        public string ObjectName { get; set; }
        public string Details { get; set; }
        public int Severity => Type switch
        {
            ChangeType.TableRemoved => 3,
            ChangeType.ColumnRemoved => 3,
            ChangeType.TableModified => 2,
            ChangeType.ColumnModified => 2,
            ChangeType.RelationshipModified => 2,
            _ => 1
        };
    }

    public class SchemaComparisonStatistics
    {
        public int TablesAdded { get; set; }
        public int TablesRemoved { get; set; }
        public int TablesModified { get; set; }
        public int ColumnsAdded { get; set; }
        public int ColumnsRemoved { get; set; }
        public int ColumnsModified { get; set; }
        public int RelationshipsAdded { get; set; }

        public int RelationshipsRemoved { get; set; }
        public int RelationshipsModified { get; set; }
        public int IndexesAdded { get; set; }
        public int IndexesRemoved { get; set; }
        public int IndexesModified { get; set; }

        public int TotalChanges =>
            TablesAdded + TablesRemoved + TablesModified +
            ColumnsAdded + ColumnsRemoved + ColumnsModified +
            RelationshipsAdded + RelationshipsRemoved + RelationshipsModified +
            IndexesAdded + IndexesRemoved + IndexesModified;

        public int BreakingChanges =>
            TablesRemoved + ColumnsRemoved + RelationshipsRemoved;
    }

    public enum ChangeType
    {
        TableAdded,
        TableRemoved,
        TableModified,
        ColumnAdded,
        ColumnRemoved,
        ColumnModified,
        RelationshipAdded,
        RelationshipRemoved,
        RelationshipModified,
        IndexAdded,
        IndexRemoved,
        IndexModified
    }
}