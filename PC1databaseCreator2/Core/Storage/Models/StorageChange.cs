using System;
using PC1databaseCreator.Core.Storage.Common.Enums;
using PC1databaseCreator.Core.Storage.Enums;

namespace PC1databaseCreator.Core.Storage.Models
{
    public record StorageChange
    {
        public string Path { get; init; }
        public StorageChangeType ChangeType { get; init; }
        public DateTime ChangeTime { get; init; } = DateTime.UtcNow;
        public StorageType StorageType { get; init; }
        public string Description { get; init; }
    }
}