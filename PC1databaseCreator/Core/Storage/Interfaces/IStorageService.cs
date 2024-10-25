using ErrorOr;

namespace PC1databaseCreator.Core.Storage.Interfaces
{
    /// <summary>
    /// 스토리지 서비스 인터페이스
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// 데이터를 저장합니다.
        /// </summary>
        Task<ErrorOr<bool>> SaveDataAsync(string name, byte[] data, CancellationToken cancellationToken = default);

        /// <summary>
        /// 데이터를 로드합니다.
        /// </summary>
        Task<ErrorOr<byte[]>> LoadDataAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// 데이터를 삭제합니다.
        /// </summary>
        Task<ErrorOr<bool>> DeleteDataAsync(string name, CancellationToken cancellationToken = default);
    }
}