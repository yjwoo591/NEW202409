using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using PC1MAINAITradingSystem.Core.ERD.Models;

namespace PC1MAINAITradingSystem.Core.ERD.Comparator
{
    public class ERDComparer
    {
        private readonly ILogger _logger;
        private readonly SchemaComparer _schemaComparer;

        public ERDComparer(ILogger logger, SchemaComparer schemaComparer)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _schemaComparer = schemaComparer ?? throw new ArgumentNullException(nameof(schemaComparer));
        }

        public async Task<ERDComparisonResult> CompareERDs(ERDModel oldERD, ERDModel newERD, ComparisonOptions options)
        {
            try
            {
                await _logger.LogInfo("Starting ERD comparison");
                var result = new ERDComparisonResult
                {
                    OldERD = oldERD,
                    NewERD = newERD,
                    ComparedAt = DateTime.UtcNow
                };

                // 스키마 비교 수행
                var schemaComparison = await _schemaComparer.CompareSchemas(oldERD, newERD);
                result.SchemaComparison = schemaComparison;

                // ERD 특정 비교 수행
                await CompareERDSpecifics(oldERD, newERD, result, options);

                // 영향도 분석
                if (options.AnalyzeImpact)
                {
                    await AnalyzeImpact(result);
                }

                // 마이그레이션 계획 생성
                if (options.GenerateMigrationPlan)
                {
                    await GenerateMigrationPlan(result);
                }

                await _logger.LogInfo("ERD comparison completed");
                return result;
            }
            catch (Exception ex)
            {
                await _logger.LogError("ERD comparison failed", ex);
                throw;
            }
        }

        private async Task CompareERDSpecifics(ERDModel oldERD, ERDModel newERD,
            ERDComparisonResult result, ComparisonOptions options)
        {
            var differences = new List<ERDDifference>();

            // 메타데이터 비교
            if (options.CompareMetadata &&
                !DictionariesAreEqual(oldERD.Metadata, newERD.Metadata))
            {
                differences.Add(new ERDDifference
                {
                    Type = DifferenceType.Metadata,
                    Description = "Metadata has changed",
                    Details = CompareMetadata(oldERD.Metadata, newERD.Metadata)
                });
            }

            // 테이블 배치 변경 비교
            if (options.CompareLayout)
            {
                var layoutChanges = CompareTableLayouts(oldERD, newERD);
                if (layoutChanges.Any())
                {
                    differences.Add(new ERDDifference
                    {
                        Type = DifferenceType.Layout,
                        Description = "Table layout has changed",
                        Details = string.Join("\n", layoutChanges)
                    });
                }
            }

            // 관계 스타일 변경 비교
            if (options.CompareStyles)
            {
                var styleChanges = CompareRelationshipStyles(oldERD, newERD);
                if (styleChanges.Any())
                {
                    differences.Add(new ERDDifference
                    {
                        Type = DifferenceType.Style,
                        Description = "Relationship styles have changed",
                        Details = string.Join("\n", styleChanges)
                    });
                }
            }

            result.Differences = differences;
        }

        private bool DictionariesAreEqual(Dictionary<string, string> dict1, Dictionary<string, string> dict2)
        {
            return dict1.Count == dict2.Count &&
                   !dict1.Except(dict2).Any();
        }

        private string CompareMetadata(Dictionary<string, string> oldMetadata, Dictionary<string, string> newMetadata)
        {
            var differences = new StringBuilder();

            // 추가된 메타데이터
            foreach (var item in newMetadata)
            {
                if (!oldMetadata.ContainsKey(item.Key))
                {
                    differences.AppendLine($"Added: {item.Key} = {item.Value}");
                }
                else if (oldMetadata[item.Key] != item.Value)
                {
                    differences.AppendLine($"Changed: {item.Key} from '{oldMetadata[item.Key]}' to '{item.Value}'");
                }
            }

            // 삭제된 메타데이터
            foreach (var item in oldMetadata)
            {
                if (!newMetadata.ContainsKey(item.Key))
                {
                    differences.AppendLine($"Removed: {item.Key} = {item.Value}");
                }
            }

            return differences.ToString();
        }

        private List<string> CompareTableLayouts(ERDModel oldERD, ERDModel newERD)
        {
            var changes = new List<string>();

            // 여기서는 실제 레이아웃 정보가 모델에 포함되어 있다고 가정합니다.
            // 실제 구현에서는 레이아웃 정보를 저장하는 방식에 따라 수정이 필요합니다.

            return changes;
        }

        private List<string> CompareRelationshipStyles(ERDModel oldERD, ERDModel newERD)
        {
            var changes = new List<string>();

            // 여기서는 실제 스타일 정보가 모델에 포함되어 있다고 가정합니다.
            // 실제 구현에서는 스타일 정보를 저장하는 방식에 따라 수정이 필요합니다.

            return changes;
        }

        private async Task AnalyzeImpact(ERDComparisonResult result)
        {
            var impact = new ImpactAnalysis();

            foreach (var change in result.SchemaComparison.Changes)
            {
                switch (change.Type)
                {
                    case ChangeType.TableRemoved:
                    case ChangeType.ColumnRemoved:
                        impact.BreakingChanges.Add(new ImpactItem
                        {
                            Severity = ImpactSeverity.High,
                            Description = change.Details,
                            AffectedObject = change.ObjectName,
                            RecommendedAction = "Data migration required"
                        });
                        break;

                    case ChangeType.TableModified:
                    case ChangeType.ColumnModified:
                        impact.PotentialIssues.Add(new ImpactItem
                        {
                            Severity = ImpactSeverity.Medium,
                            Description = change.Details,
                            AffectedObject = change.ObjectName,
                            RecommendedAction = "Data validation required"
                        });
                        break;

                    case ChangeType.RelationshipModified:
                        impact.PotentialIssues.Add(new ImpactItem
                        {
                            Severity = ImpactSeverity.Medium,
                            Description = change.Details,
                            AffectedObject = change.ObjectName,
                            RecommendedAction = "Referential integrity check required"
                        });
                        break;

                    default:
                        impact.Notifications.Add(new ImpactItem
                        {
                            Severity = ImpactSeverity.Low,
                            Description = change.Details,
                            AffectedObject = change.ObjectName,
                            RecommendedAction = "No immediate action required"
                        });
                        break;
                }
            }

            result.ImpactAnalysis = impact;
        }

        private async Task GenerateMigrationPlan(ERDComparisonResult result)
        {
            var plan = new MigrationPlan
            {
                CreatedAt = DateTime.UtcNow,
                Steps = new List<MigrationStep>()
            };

            // 1. 사전 검사 단계
            plan.Steps.Add(new MigrationStep
            {
                Order = 1,
                Type = MigrationStepType.Validation,
                Description = "Perform pre-migration validation",
                SqlCommands = new[] { "-- Validation queries will be generated here" }
            });

            // 2. 백업 단계
            plan.Steps.Add(new MigrationStep
            {
                Order = 2,
                Type = MigrationStepType.Backup,
                Description = "Create database backup",
                SqlCommands = new[] { "-- Backup commands will be generated here" }
            });

            // 3. 스키마 변경 단계
            var schemaChanges = result.SchemaComparison.Changes
                .OrderBy(c => c.Severity)
                .GroupBy(c => c.Type);

            int stepOrder = 3;
            foreach (var changeGroup in schemaChanges)
            {
                plan.Steps.Add(new MigrationStep
                {
                    Order = stepOrder++,
                    Type = MigrationStepType.Schema,
                    Description = $"Apply {changeGroup.Key} changes",
                    SqlCommands = changeGroup.Select(c => $"-- SQL for: {c.Details}").ToArray()
                });
            }

            // 4. 데이터 마이그레이션 단계
            plan.Steps.Add(new MigrationStep
            {
                Order = stepOrder++,
                Type = MigrationStepType.Data,
                Description = "Migrate existing data",
                SqlCommands = new[] { "-- Data migration commands will be generated here" }
            });

            // 5. 검증 단계
            plan.Steps.Add(new MigrationStep
            {
                Order = stepOrder,
                Type = MigrationStepType.Validation,
                Description = "Perform post-migration validation",
                SqlCommands = new[] { "-- Post-migration validation queries will be generated here" }
            });

            result.MigrationPlan = plan;
        }
    }

    public class ERDComparisonResult
    {
        public ERDModel OldERD { get; set; }
        public ERDModel NewERD { get; set; }
        public DateTime ComparedAt { get; set; }
        public SchemaComparisonResult SchemaComparison { get; set; }
        public List<ERDDifference> Differences { get; set; }
        public ImpactAnalysis ImpactAnalysis { get; set; }
        public MigrationPlan MigrationPlan { get; set; }
    }

    public class ERDDifference
    {
        public DifferenceType Type { get; set; }
        public string Description { get; set; }
        public string Details { get; set; }
    }

    public class ImpactAnalysis
    {
        public List<ImpactItem> BreakingChanges { get; set; } = new();
        public List<ImpactItem> PotentialIssues { get; set; } = new();
        public List<ImpactItem> Notifications { get; set; } = new();
    }

    public class ImpactItem
    {
        public ImpactSeverity Severity { get; set; }
        public string Description { get; set; }
        public string AffectedObject { get; set; }
        public string RecommendedAction { get; set; }
    }

    public class MigrationPlan
    {
        public DateTime CreatedAt { get; set; }
        public List<MigrationStep> Steps { get; set; }
        public TimeSpan EstimatedDuration => TimeSpan.FromMinutes(Steps.Count * 5); // 예상 시간
    }

    public class MigrationStep
    {
        public int Order { get; set; }
        public MigrationStepType Type { get; set; }
        public string Description { get; set; }
        public string[] SqlCommands { get; set; }
        public bool IsReversible { get; set; }
        public string[] RollbackCommands { get; set; }
    }

    public enum DifferenceType
    {
        Metadata,
        Layout,
        Style,
        Documentation
    }

    public enum ImpactSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum MigrationStepType
    {
        Validation,
        Backup,
        Schema,
        Data,
        Index,
        Constraint
    }

    public class ComparisonOptions
    {
        public bool CompareMetadata { get; set; } = true;
        public bool CompareLayout { get; set; } = true;
        public bool CompareStyles { get; set; } = true;
        public bool AnalyzeImpact { get; set; } = true;
        public bool GenerateMigrationPlan { get; set; } = true;
        public List<string> ExcludedTables { get; set; } = new();
        public bool IgnoreCase { get; set; } = true;
        public bool IgnoreWhitespace { get; set; } = true;
    }
}