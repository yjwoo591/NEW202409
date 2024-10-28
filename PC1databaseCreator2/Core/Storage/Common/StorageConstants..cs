namespace PC1databaseCreator.Core.Storage.Common
{
    public static class StorageConstants
    {
        public static class Paths
        {
            public const string FastAccess = "FastAccess";
            public const string Archive = "Archive";
            public const string Temp = "Temp";
        }

        public static class Extensions
        {
            public const string Dat = ".dat";
            public const string Tmp = ".tmp";
            public const string Bak = ".bak";
        }

        public static class Sizes
        {
            public const long MaximumFileSize = 1L * 1024L * 1024L * 1024L; // 1GB
            public const long FastAccessThreshold = 100L * 1024L * 1024L;    // 100MB
            public const long MinimumFreeSpace = 50L * 1024L * 1024L * 1024L; // 50GB
        }

        public static class Timeouts
        {
            public const int DefaultOperationTimeout = 30000;    // 30 seconds
            public const int ExtendedOperationTimeout = 300000;  // 5 minutes
        }
    }
}