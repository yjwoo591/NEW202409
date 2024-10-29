namespace PC1databaseCreator.Common.Infrastructure.Enums
{
    /// <summary>
    /// 스토리지 변경 유형 열거형
    /// </summary>
    public enum StorageChangeType
    {
        /// <summary>
        /// 추가
        /// </summary>
        Add,

        /// <summary>
        /// 수정
        /// </summary>
        Update,

        /// <summary>
        /// 삭제
        /// </summary>
        Delete,

        /// <summary>
        /// 이동
        /// </summary>
        Move,

        /// <summary>
        /// 백업
        /// </summary>
        Backup,

        /// <summary>
        /// 복원
        /// </summary>
        Restore,

        /// <summary>
        /// 초기화
        /// </summary>
        Initialize
    }
}