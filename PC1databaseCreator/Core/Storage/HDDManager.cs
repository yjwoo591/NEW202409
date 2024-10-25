using Microsoft.Extensions.Logging;
using PC1databaseCreator.Common.Infrastructure.Storage;
using System.Collections.Concurrent;

namespace PC1databaseCreator.Core.Storage
{
    public class HDDManager : AbstractStorageManager, IDisposable
    {
        private readonly HDDConfig _config;
        private readonly ILogger<HDDManager> _logger;
        private readonly ConcurrentDictionary<string, DriveUsageInfo> _driveUsage;
        private int _currentDriveIndex;
        private readonly object _driveLock = new object();
        private bool _disposed;

        private const string FAST_ACCESS_FOLDER = "FastAccess";
        private const string ARCHIVE_FOLDER = "Archive";
        private const int MIN_FREE_SPACE_GB = 50;

        public HDDManager(HDDConfig config, ILogger<HDDManager> logger)
            : base(
                config?.PrimaryDrives?.FirstOrDefault()?.Path
                    ?? throw new ArgumentNullException(nameof(config), "Config or primary drives cannot be null"),
                StorageType.HDD)
        {
            _config = config;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _driveUsage = new ConcurrentDictionary<string, DriveUsageInfo>();
            _currentDriveIndex = 0;

            InitializeStorageStructure();
            _logger.LogInformation("HDDManager initialized successfully");
        }

        private void InitializeStorageStructure()
        {
            try
            {
                // Primary 드라이브 초기화
                foreach (var drive in _config.PrimaryDrives)
                {
                    InitializeDriveDirectories(drive.Path, "Primary");
                    _driveUsage[drive.Path] = new DriveUsageInfo();
                }

                // Mirror 드라이브 초기화
                foreach (var drive in _config.MirrorDrives)
                {
                    InitializeDriveDirectories(drive.Path, "Mirror");
                    _driveUsage[drive.Path] = new DriveUsageInfo();
                }

                _logger.LogInformation("Storage structure initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize storage structure");
                throw new StorageException("Failed to initialize storage structure", Type, ex);
            }
        }

        private void InitializeDriveDirectories(string drivePath, string driveType)
        {
            try
            {
                var fastAccessPath = Path.Combine(drivePath, FAST_ACCESS_FOLDER);
                var archivePath = Path.Combine(drivePath, ARCHIVE_FOLDER);

                EnsureDirectoryExists(fastAccessPath);
                EnsureDirectoryExists(archivePath);

                _logger.LogInformation("Initialized {DriveType} drive directories at: {Path}",
                    driveType, drivePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize {DriveType} drive directories at: {Path}",
                    driveType, drivePath);
                throw;
            }
        }

        public override async Task<bool> SaveDataAsync(string name, byte[] data)
        {
            ThrowIfDisposed();
            try
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentException("File name cannot be empty", nameof(name));

                if (data == null || data.Length == 0)
                    throw new ArgumentException("Data cannot be empty", nameof(data));

                var folder = DetermineStorageFolder(data.Length);
                var targetDrive = SelectOptimalDrive(data.Length);

                var primaryPath = Path.Combine(targetDrive.Path, folder, name);
                var mirrorPath = Path.Combine(
                    _config.MirrorDrives[targetDrive.DriveNumber].Path,
                    folder,
                    name
                );

                await SaveWithMirrorAsync(primaryPath, mirrorPath, data);
                UpdateDriveUsage(targetDrive.Path, data.Length);

                _logger.LogInformation("Successfully saved file: {Name} to {Path}", name, primaryPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save file: {Name}", name);
                throw;
            }
        }

        public override async Task<byte[]> LoadDataAsync(string name)
        {
            ThrowIfDisposed();
            try
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentException("File name cannot be empty", nameof(name));

                foreach (var drive in _config.PrimaryDrives)
                {
                    // FastAccess 폴더에서 먼저 검색
                    var fastPath = Path.Combine(drive.Path, FAST_ACCESS_FOLDER, name);
                    if (File.Exists(fastPath))
                    {
                        var data = await File.ReadAllBytesAsync(fastPath);
                        _logger.LogInformation("Successfully loaded file: {Name} from FastAccess", name);
                        return data;
                    }

                    // Archive 폴더에서 검색
                    var archivePath = Path.Combine(drive.Path, ARCHIVE_FOLDER, name);
                    if (File.Exists(archivePath))
                    {
                        var data = await File.ReadAllBytesAsync(archivePath);
                        _logger.LogInformation("Successfully loaded file: {Name} from Archive", name);
                        return data;
                    }
                }

                var message = $"File not found: {name}";
                _logger.LogWarning(message);
                throw new FileNotFoundException(message);
            }
            catch (Exception ex) when (ex is not FileNotFoundException)
            {
                _logger.LogError(ex, "Failed to load file: {Name}", name);
                throw;
            }
        }

        private async Task SaveWithMirrorAsync(string primaryPath, string mirrorPath, byte[] data)
        {
            try
            {
                await Task.WhenAll(
                    File.WriteAllBytesAsync(primaryPath, data),
                    File.WriteAllBytesAsync(mirrorPath, data)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save file with mirror");
                CleanupFailedSave(primaryPath, mirrorPath);
                throw;
            }
        }

        private void CleanupFailedSave(string primaryPath, string mirrorPath)
        {
            try
            {
                if (File.Exists(primaryPath)) File.Delete(primaryPath);
                if (File.Exists(mirrorPath)) File.Delete(mirrorPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup after failed save");
            }
        }

        private string DetermineStorageFolder(long size)
            => size < 100 * 1024 * 1024 ? FAST_ACCESS_FOLDER : ARCHIVE_FOLDER;

        private DriveInfo SelectOptimalDrive(long dataSize)
        {
            lock (_driveLock)
            {
                var drive = _config.PrimaryDrives[_currentDriveIndex];
                _currentDriveIndex = (_currentDriveIndex + 1) % _config.PrimaryDrives.Count;
                return drive;
            }
        }

        private void UpdateDriveUsage(string drivePath, long dataSize)
        {
            var usage = _driveUsage.GetOrAdd(drivePath, new DriveUsageInfo());
            usage.UsedSpace += dataSize;
            usage.LastUpdated = DateTime.UtcNow;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _driveUsage.Clear();
                _logger.LogInformation("HDDManager disposed successfully");
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~HDDManager()
        {
            Dispose(false);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(HDDManager));
            }
        }
    }

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
    }

    public class DriveUsageInfo
    {
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public long UsedSpace { get; set; }
        public long WriteCount { get; set; }
    }
}