namespace PC1databaseCreator.Common.Infrastructure.Storage
{
    public abstract class AbstractStorageManager : IStorageManager
    {
        protected readonly string BasePath;
        public StorageType Type { get; }

        protected AbstractStorageManager(string basePath, StorageType type)
        {
            BasePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
            Type = type;

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
        }

        public abstract Task<bool> SaveDataAsync(string name, byte[] data);
        public abstract Task<byte[]> LoadDataAsync(string name);

        protected string CombinePath(params string[] paths)
        {
            var allPaths = new List<string> { BasePath };
            allPaths.AddRange(paths);
            return Path.Combine(allPaths.ToArray());
        }

        protected bool IsPathInStorage(string path)
        {
            var fullPath = Path.GetFullPath(path);
            var storagePath = Path.GetFullPath(BasePath);
            return fullPath.StartsWith(storagePath);
        }

        protected void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}