using System.Text.RegularExpressions;

namespace PC1databaseCreator.Core.Storage.Common
{
    public class FileName
    {
        private static readonly Regex InvalidCharsRegex = new Regex(
            $"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]",
            RegexOptions.Compiled);

        public string Name { get; }
        public string Extension { get; }
        public string FullName => $"{Name}{Extension}";

        public FileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            if (InvalidCharsRegex.IsMatch(fileName))
                throw new ArgumentException("File name contains invalid characters", nameof(fileName));

            Name = Path.GetFileNameWithoutExtension(fileName);
            Extension = Path.GetExtension(fileName);

            if (string.IsNullOrEmpty(Name))
                throw new ArgumentException("File name is empty", nameof(fileName));
        }

        public string ToTempName()
        {
            return $"{Name}_{DateTime.UtcNow:yyyyMMddHHmmss}{StorageConstants.Extensions.Tmp}";
        }

        public string ToBackupName()
        {
            return $"{Name}_{DateTime.UtcNow:yyyyMMddHHmmss}{StorageConstants.Extensions.Bak}";
        }

        public string GetPathInFolder(string basePath, string folder)
        {
            return Path.Combine(basePath, folder, FullName);
        }

        public bool IsInFolder(string basePath, string folder)
        {
            var normalizedPath = Path.GetFullPath(Path.Combine(basePath, folder));
            var filePath = Path.GetFullPath(Path.Combine(basePath, folder, FullName));
            return filePath.StartsWith(normalizedPath, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return FullName;
        }

        public override bool Equals(object? obj)
        {
            if (obj is FileName other)
            {
                return string.Equals(FullName, other.FullName, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(FullName);
        }
    }
}