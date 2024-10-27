using System.IO;
using System.Collections.Concurrent;
using PC1databaseCreator.Common.Infrastructure.Interfaces.Logging;
using PC1databaseCreator.Common.Results;
using PC1databaseCreator.Core.Storage.Models;

namespace PC1databaseCreator.Core.Storage
{
    public sealed class HDDManager : StorageManager
    {
        private const string FAST_ACCESS_FOLDER = "FastAccess";
        private const string ARCHIVE_FOLDER = "Archive";
        private const int MIN_FREE_SPACE_GB = 50;

        private readonly HDDConfig _config;
        private readonly ConcurrentDictionary<string, DriveUsageInfo> _driveUsage;
        private int _currentDriveIndex = 0;
        private readonly object _driveLock = new object();

        public HDDManager(HDDConfig config, ILoggerService logger)
            : base(config.PrimaryDrives[0].Path, logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _driveUsage = new ConcurrentDictionary<string, DriveUsageInfo>();
            InitializeStorage();
        }

        private void InitializeStorage()
        {
            foreach (var drive in _config.PrimaryDrives)
            {
                Directory.CreateDirectory(Path.Combine(drive.Path, FAST_ACCESS_FOLDER));
                Directory.CreateDirectory(Path.Combine(drive.Path, ARCHIVE_FOLDER));
                _driveUsage[drive.Path] = new DriveUsageInfo();
            }

            foreach (var drive in _config.MirrorDrives)
            {
                Directory.CreateDirectory(Path.Combine(drive.Path, FAST_ACCESS_FOLDER));
                Directory.CreateDirectory(Path.Combine(drive.Path, ARCHIVE_FOLDER));
                _driveUsage[drive.Path] = new DriveUsageInfo();
            }
        }

        public override async Task<Result<byte[]>> ReadAsync(string fileName)
        {
            // 먼저 FastAccess 폴더에서 검색
            foreach (var drive in _config.PrimaryDrives)
            {
                var fastPath = Path.Combine(drive.Path, FAST_ACCESS_FOLDER, fileName);
                if (File.Exists(fastPath))
                {
                    return await base.ReadAsync(fastPath);
                }

                var archivePath = Path.Combine(drive.Path, ARCHIVE_FOLDER, fileName);
                if (File.Exists(archivePath))
                {
                    return await base.ReadAsync(archivePath);
                }
            }

            return Result<byte[]>.Failure($"File not found: {fileName}");
        }

        public override async Task<Result> WriteAsync(string fileName, byte[] data)
        {
            if (data == null)
                return Result.Failure("Data cannot be null");

            var targetDrive = SelectOptimalDrive(data.Length);
            if (targetDrive == null)
                return Result.Failure("No suitable drive available");

            try
            {
                string folder = DetermineStorageFolder(data.Length);
                string primaryPath = Path.Combine(targetDrive.Path, folder, fileName);
                string mirrorPath = Path.Combine(
                    _config.MirrorDrives[targetDrive.DriveNumber].Path,
                    folder,
                    fileName);

                var primaryResult = await base.WriteAsync(primaryPath, data);
                if (!primaryResult.IsSuccess)
                    return primaryResult;

                var mirrorResult = await base.WriteAsync(mirrorPath, data);
                if (!mirrorResult.IsSuccess)
                {
                    await base.DeleteAsync(primaryPath);
                    return mirrorResult;
                }

                UpdateDriveUsage(targetDrive.Path, data.Length);
                return Result.Success();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to write file: {FileName}", fileName);
                return Result.Failure(ex);
            }
        }

        private DriveConfiguration SelectOptimalDrive(long dataSize)
        {
            lock (_driveLock)
            {
                foreach (var drive in _config.PrimaryDrives)
                {
                    var driveInfo = new DriveInfo(Path.GetPathRoot(drive.Path));
                    if (driveInfo.AvailableFreeSpace > dataSize + (MIN_FREE_SPACE_GB * 1024L * 1024L * 1024L))
                    {
                        return drive;
                    }
                }
                return null;
            }
        }

        private string DetermineStorageFolder(long size)
        {
            return size < 100 * 1024 * 1024 ? FAST_ACCESS_FOLDER : ARCHIVE_FOLDER;
        }

        private void UpdateDriveUsage(string drivePath, long dataSize)
        {
            var usage = _driveUsage.GetOrAdd(drivePath, new DriveUsageInfo());
            usage.WriteCount++;
            usage.LastWrite = DateTime.Now;
            usage.UsedSpace += dataSize;
        }
    }

    public class DriveUsageInfo
    {
        public DateTime LastWrite { get; set; } = DateTime.Now;
        public long WriteCount { get; set; }
        public long UsedSpace { get; set; }
    }

    public class HDDConfig
    {
        public List<DriveConfiguration> PrimaryDrives { get; set; } = new();
        public List<DriveConfiguration> MirrorDrives { get; set; } = new();
    }
}