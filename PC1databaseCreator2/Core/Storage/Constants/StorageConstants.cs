namespace PC1databaseCreator.Core.Storage.Constants
{
    public static class StorageConstants
    {
        public const int DEFAULT_BUFFER_SIZE = 81920; // 80KB
        public const int LARGE_FILE_THRESHOLD = 1048576; // 1MB

        public static class Cache
        {
            public const int DEFAULT_CACHE_MINUTES = 30;
            public const int MAX_CACHE_ITEMS = 1000;
            public const long MAX_CACHE_SIZE = 104857600; // 100MB
        }

        public static class Monitoring
        {
            public const int CRITICAL_SPACE_THRESHOLD = 90; // 90% used
            public const int WARNING_SPACE_THRESHOLD = 80; // 80% used
            public const int MIN_FREE_SPACE_GB = 50;
        }

        public static class Paths
        {
            public const string FAST_ACCESS_FOLDER = "FastAccess";
            public const string ARCHIVE_FOLDER = "Archive";
            public const string TEMP_FOLDER = "Temp";
        }

        public static class ErrorMessages
        {
            public const string INSUFFICIENT_SPACE = "Insufficient storage space available";
            public const string PATH_NOT_FOUND = "Specified path not found";
            public const string ACCESS_DENIED = "Access denied to storage location";
            public const string INVALID_OPERATION = "Invalid storage operation";
        }
    }
}