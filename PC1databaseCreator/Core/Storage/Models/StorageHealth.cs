using System;
using System.Collections.Generic;
using PC1databaseCreator.Core.Storage.Common.Enums;
using PC1databaseCreator.Core.Storage.Enums;

namespace PC1databaseCreator.Core.Storage.Models
{
    public record StorageHealth
    {
        public StorageType StorageType { get; init; }
        public StorageStatus Status { get; init; }
        public List<string> Messages { get; init; } = new();
        public DateTime CheckTime { get; init; } = DateTime.UtcNow;
        public StorageMetrics Metrics { get; init; }
        public bool IsOperational => Status != StorageStatus.Critical && Status != StorageStatus.Error;
    }
}