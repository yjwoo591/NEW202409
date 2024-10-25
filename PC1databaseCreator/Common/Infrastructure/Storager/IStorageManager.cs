using System.Threading.Tasks;

namespace PC1databaseCreator.Common.Infrastructure.Storage
{
    /// <summary>
    /// 스토리지 관리자를 위한 인터페이스
    /// </summary>
    public interface IStorageManager
    {
        /// <summary>
        /// 데이터를 저장합니다.
        /// </summary>
        Task<bool> SaveDataAsync(string name, byte[] data);

        /// <summary>
        /// 데이터를 로드합니다.
        /// </summary>
        Task<byte[]> LoadDataAsync(string name);
    }
}