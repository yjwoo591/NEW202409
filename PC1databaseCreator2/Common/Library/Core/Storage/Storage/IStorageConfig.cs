namespace PC1databaseCreator.Common.Library.Core.Storage
{
    /// <summary>
    /// 스토리지 설정 인터페이스
    /// </summary>
    public interface IStorageConfig
    {
        /// <summary>
        /// 설정의 유효성을 검사
        /// </summary>
        /// <returns>유효성 검사 결과</returns>
        bool Validate();

        /// <summary>
        /// 새로운 드라이브 쌍을 추가
        /// </summary>
        /// <param name="primaryPath">주 드라이브 경로</param>
        /// <param name="mirrorPath">미러 드라이브 경로</param>
        void AddDrivePair(string primaryPath, string mirrorPath);
    }
}