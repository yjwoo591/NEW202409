namespace PC1databaseCreator.Core.Storage.Models
{
    public class HDDConfig
    {
        public List<DriveInfo> PrimaryDrives { get; set; } = new List<DriveInfo>();
        public List<DriveInfo> MirrorDrives { get; set; } = new List<DriveInfo>();

        public void AddDrivePair(string primaryPath, string mirrorPath)
        {
            var primaryDriveNumber = PrimaryDrives.Count;
            PrimaryDrives.Add(new DriveInfo
            {
                Path = primaryPath,
                DriveNumber = primaryDriveNumber
            });

            MirrorDrives.Add(new DriveInfo
            {
                Path = mirrorPath,
                DriveNumber = primaryDriveNumber
            });
        }
    }

    public class DriveInfo
    {
        public string Path { get; set; }
        public int DriveNumber { get; set; }

        public DriveUsageInfo GetUsageInfo()
        {
            var sysInfo = new System.IO.DriveInfo(System.IO.Path.GetPathRoot(Path));
            return new DriveUsageInfo
            {
                TotalSpace = sysInfo.TotalSize,
                AvailableSpace = sysInfo.AvailableFreeSpace,
                UsedSpace = sysInfo.TotalSize - sysInfo.AvailableFreeSpace
            };
        }
    }

    public class DriveUsageInfo
    {
        public long TotalSpace { get; set; }
        public long AvailableSpace { get; set; }
        public long UsedSpace { get; set; }
        public double UsedSpacePercentage => (double)UsedSpace / TotalSpace * 100;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}