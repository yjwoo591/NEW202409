namespace PC1databaseCreator.Common.Library.Core.Storage
{
    /// <summary>
    /// 스토리지 유형을 정의하는 열거형
    /// </summary>
    public enum StorageType
    {
        /// <summary>
        /// 주 SSD 스토리지
        /// </summary>
        SSD1,

        /// <summary>
        /// 보조 SSD 스토리지
        /// </summary>
        SSD2,

        /// <summary>
        /// HDD 스토리지
        /// </summary>
        HDD
    }

    /// <summary>
    /// 드라이브 상태를 정의하는 열거형
    /// </summary>
    public enum DriveStatus
    {
        /// <summary>
        /// 알 수 없음
        /// </summary>
        Unknown,

        /// <summary>
        /// 준비됨
        /// </summary>
        Ready,

        /// <summary>
        /// 초기화 중
        /// </summary>
        Initializing,

        /// <summary>
        /// 사용 중
        /// </summary>
        InUse,

        /// <summary>
        /// 오류
        /// </summary>
        Error,

        /// <summary>
        /// 오프라인
        /// </summary>
        Offline
    }
}