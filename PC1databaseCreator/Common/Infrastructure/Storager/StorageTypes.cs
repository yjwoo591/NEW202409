namespace PC1databaseCreator.Common.Infrastructure.Storage
{
    /// <summary>
    /// 스토리지 타입을 정의하는 열거형
    /// </summary>
    public enum StorageType
    {
        /// <summary>
        /// 고성능 실시간 저장소
        /// </summary>
        SSD1,

        /// <summary>
        /// 백업용 저장소
        /// </summary>
        SSD2,

        /// <summary>
        /// 아카이브 저장소
        /// </summary>
        HDD
    }
}