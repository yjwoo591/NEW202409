namespace PC1databaseCreator.Core.Storage.Models
{
    public enum StorageType
    {
        SSD1,
        SSD2,
        HDD
    }

    public enum StorageStatus
    {
        OK,
        Warning,
        Critical,
        Error
    }

    public enum StorageOperationType
    {
        Read,
        Write,
        Delete,
        Backup,
        Restore
    }

    public enum StorageChangeType
    {
        Created,
        Modified,
        Deleted,
        Renamed
    }
}