```csharp
using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Management.Smo;
using PC1MAINAITradingSystem.Interfaces;
using PC1MAINAITradingSystem.Models;

namespace PC1MAINAITradingSystem.Core.ERDProcessor
{
    public class ERDProcessor : IERDProcessor
    {
        private readonly ILogger _logger;
        private readonly IERDReader _erdReader;
        private readonly IERDParser _erdParser;
        private readonly IERDValidator _erdValidator;
        private ERDStructure _currentERD;

        public ERDProcessor(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _erdReader = new ERDReader(logger);
            _erdParser = new ERDParser(logger);
            _erdValidator = new ERDValidator(logger);
        }

        public ERDStructure LoadAndParseERD(string filePath)
        {
            try
            {
                _logger.Log($"Loading ERD file: {filePath}");
                string erdContent = _erdReader.ReadERDFile(filePath);

                _logger.Log("Parsing ERD content");
                _currentERD = _erdParser.ParseERD(erdContent);

                _logger.Log("Validating ERD structure");
                var validationResult = _erdValidator.ValidateERD(_currentERD);

                if (!validationResult.IsValid)
                {
                    throw new ERDValidationException(validationResult.ErrorMessages);
                }

                return _currentERD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing ERD: {ex.Message}");
                throw new ERDProcessingException("Failed to process ERD file", ex);
            }
        }

        public ERDStructure GetCurrentERD()
        {
            if (_currentERD == null)
            {
                throw new InvalidOperationException("No ERD has been loaded");
            }
            return _currentERD;
        }

        public List<DatabaseChange> CompareERD(ERDStructure newERD)
        {
            if (_currentERD == null)
            {
                throw new InvalidOperationException("No current ERD loaded for comparison");
            }

            try
            {
                _logger.Log("Comparing ERD structures");
                var changes = new List<DatabaseChange>();

                // 새로운 테이블 검사
                foreach (var newTable in newERD.Tables)
                {
                    var currentTable = _currentERD.Tables.Find(t => t.Name == newTable.Name);
                    if (currentTable == null)
                    {
                        changes.Add(new DatabaseChange
                        {
                            ChangeType = ChangeType.AddTable,
                            TableName = newTable.Name,
                            TableInfo = newTable
                        });
                        continue;
                    }

                    // 컬럼 변경 검사
                    foreach (var newColumn in newTable.Columns)
                    {
                        var currentColumn = currentTable.Columns.Find(c => c.Name == newColumn.Name);
                        if (currentColumn == null)
                        {
                            changes.Add(new DatabaseChange
                            {
                                ChangeType = ChangeType.AddColumn,
                                TableName = newTable.Name,
                                ColumnInfo = newColumn
                            });
                        }
                        else if (!currentColumn.Equals(newColumn))
                        {
                            changes.Add(new DatabaseChange
                            {
                                ChangeType = ChangeType.ModifyColumn,
                                TableName = newTable.Name,
                                ColumnInfo = newColumn,
                                OldColumnInfo = currentColumn
                            });
                        }
                    }

                    // 삭제된 컬럼 검사
                    foreach (var currentColumn in currentTable.Columns)
                    {
                        if (!newTable.Columns.Exists(c => c.Name == currentColumn.Name))
                        {
                            changes.Add(new DatabaseChange
                            {
                                ChangeType = ChangeType.RemoveColumn,
                                TableName = newTable.Name,
                                ColumnInfo = currentColumn
                            });
                        }
                    }
                }

                // 삭제된 테이블 검사
                foreach (var currentTable in _currentERD.Tables)
                {
                    if (!newERD.Tables.Exists(t => t.Name == currentTable.Name))
                    {
                        changes.Add(new DatabaseChange
                        {
                            ChangeType = ChangeType.RemoveTable,
                            TableName = currentTable.Name,
                            TableInfo = currentTable
                        });
                    }
                }

                _logger.Log($"Found {changes.Count} changes in ERD comparison");
                return changes;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error comparing ERD structures: {ex.Message}");
                throw new ERDProcessingException("Failed to compare ERD structures", ex);
            }
        }

        public void UpdateCurrentERD(ERDStructure newERD)
        {
            try
            {
                _logger.Log("Updating current ERD");
                var validationResult = _erdValidator.ValidateERD(newERD);
                if (!validationResult.IsValid)
                {
                    throw new ERDValidationException(validationResult.ErrorMessages);
                }

                _currentERD = newERD;
                _logger.Log("Current ERD updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating current ERD: {ex.Message}");
                throw new ERDProcessingException("Failed to update current ERD", ex);
            }
        }
    }
}
```