namespace PC1databaseCreator.Core.Storage.Common.Enums
{
    /// <summary>
    /// HDD 상태
    /// </summary>
    public enum HDDStatus
    {
        /// <summary>
        /// 정상 동작
        /// </summary>
        Active,

        /// <summary>
        /// 경고 상태
        /// </summary>
        Warning,

        /// <summary>
        /// 오류 상태
        /// </summary>
        Error,

        /// <summary>
        /// 오프라인
        /// </summary>
        Offline
    }

    /// <summary>
    /// 작업 우선순위
    /// </summary>
    public enum OperationPriority
    {
        /// <summary>
        /// 낮은 우선순위
        /// </summary>
        Low,

        /// <summary>
        /// 일반 우선순위
        /// </summary>
        Normal,

        /// <summary>
        /// 높은 우선순위
        /// </summary>
        High,

        /// <summary>
        /// 긴급 우선순위
        /// </summary>
        Critical
    }

    /// <summary>
    /// 스토리지 유형
    /// </summary>
    public enum StorageType
    {
        /// <summary>
        /// 빠른 접근 영역
        /// </summary>
        FastAccess,

        /// <summary>
        /// 아카이브 영역
        /// </summary>
        Archive,

        /// <summary>
        /// 백업 영역
        /// </summary>
        Backup
    }

    /// <summary>
    /// 백업 상태
    /// </summary>
    public enum BackupStatus
    {
        /// <summary>
        /// 대기 중
        /// </summary>
        Pending,

        /// <summary>
        /// 진행 중
        /// </summary>
        InProgress,

        /// <summary>
        /// 완료됨
        /// </summary>
        Completed,

        /// <summary>
        /// 실패
        /// </summary>
        Failed,

        /// <summary>
        /// 취소됨
        /// </summary>
        Cancelled
    }
}