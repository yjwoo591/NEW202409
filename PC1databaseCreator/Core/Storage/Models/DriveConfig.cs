using PC1databaseCreator.Core.Storage.Common.Enums;
using PC1databaseCreator.Core.Storage.Enums;

namespace PC1databaseCreator.Core.Storage.Models
{
    public record DriveConfig
    {
        public string Path { get; init; }
        public StorageType StorageType { get; init; }
        public int DriveNumber { get; init; }
        public bool IsPrimary { get; init; }
        public bool IsMirror { get; init; }
        public long MinimumFreeSpace { get; init; }
        public string Description { get; init; }
    }
}