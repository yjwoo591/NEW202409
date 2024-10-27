namespace PC1databaseCreator.Core.Storage.Models
{
    public class DriveConfiguration
    {
        public string Path { get; set; } = string.Empty;
        public int DriveNumber { get; set; }
        public long MinimumFreeSpace { get; set; } = 50L * 1024L * 1024L * 1024L; // 50GB
    }
}