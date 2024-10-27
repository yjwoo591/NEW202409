namespace PC1databaseCreator.Core.Storage.Common.Constants
{
    /// <summary>
    /// 스토리지 관련 상수
    /// </summary>
    public static class StorageConstants
    {
        #region Space Constants
        /// <summary>
        /// 최소 여유 공간 (GB)
        /// </summary>
        public const int MIN_FREE_SPACE_GB = 50;

        /// <summary>
        /// 위험 공간 임계값 (%)
        /// </summary>
        public const double CRITICAL_SPACE_THRESHOLD = 90.0;

        /// <summary>
        /// 버퍼 크기 (80KB)
        /// </summary>
        public const int BUFFER_SIZE = 81920;
        #endregion

        #region Performance Constants
        /// <summary>
        /// 최소 허용 IOPS
        /// </summary>
        public const int MIN_ACCEPTABLE_IOPS = 50;

        /// <summary>
        /// 최대 허용 지연시간 (ms)
        /// </summary>
        public const double MAX_ACCEPTABLE_LATENCY_MS = 20.0;

        /// <summary>
        /// 최소 허용 처리량 (MB/s)
        /// </summary>
        public const double MIN_THROUGHPUT_MBPS = 100.0;
        #endregion

        #region Path Constants
        /// <summary>
        /// 빠른 접근 폴더명
        /// </summary>
        public const string FAST_ACCESS_FOLDER = "FastAccess";

        /// <summary>
        /// 아카이브 폴더명
        /// </summary>
        public const string ARCHIVE_FOLDER = "Archive";

        /// <summary>
        /// 백업 폴더명
        /// </summary>
        public const string BACKUP_FOLDER = "Backup";
        #endregion

        #region Time Constants
        /// <summary>
        /// 기본 타임아웃 (초)
        /// </summary>
        public const int DEFAULT_TIMEOUT_SECONDS = 30;

        /// <summary>
        /// 최대 재시도 횟수
        /// </summary>
        public const int MAX_RETRY_COUNT = 3;

        /// <summary>
        /// 재시도 지연시간 (ms)
        /// </summary>
        public const int RETRY_DELAY_MS = 1000;

        /// <summary>
        /// 아카이브 기간 (일)
        /// </summary>
        public const int ARCHIVE_DAYS = 90;
        #endregion
    }
}