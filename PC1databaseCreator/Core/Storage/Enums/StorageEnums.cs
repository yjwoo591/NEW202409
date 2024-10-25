namespace PC1databaseCreator.Core.Storage.Enums
{
    public enum StorageType
    {
        SSD1,
        SSD2,
        HDD
    }

    public enum StorageStatus
    {
        Ready,
        Busy,
        Error,
        Maintenance,
        Offline
    }

    public enum StorageOperation
    {
        Read,
        Write,
        Delete,
        Backup,
        Restore
    }
}