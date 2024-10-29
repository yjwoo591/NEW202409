using System;
using System.Threading.Tasks;
using ErrorOr;

namespace PC1databaseCreator.Common.Library.Core.Storage
{
    /// <summary>
    /// 추상 스토리지 관리자 기본 클래스
    /// </summary>
    public abstract class AbstractStorageManager
    {
        /// <summary>
        /// 데이터 저장
        /// </summary>
        /// <param name="path">저장 경로</param>
        /// <param name="data">저장할 데이터</param>
        /// <returns>성공 또는 오류</returns>
        public abstract Task<ErrorOr<Success>> SaveDataAsync(string path, byte[] data);

        /// <summary>
        /// 데이터 로드
        /// </summary>
        /// <param name="path">로드할 파일 경로</param>
        /// <returns>데이터 또는 오류</returns>
        public abstract Task<ErrorOr<byte[]>> LoadDataAsync(string path);

        /// <summary>
        /// 데이터 삭제
        /// </summary>
        /// <param name="path">삭제할 파일 경로</param>
        /// <returns>성공 또는 오류</returns>
        public abstract ErrorOr<Success> DeleteData(string path);

        /// <summary>
        /// 데이터 존재 여부 확인
        /// </summary>
        /// <param name="path">확인할 파일 경로</param>
        /// <returns>존재 여부</returns>
        public abstract bool Exists(string path);

        /// <summary>
        /// 스토리지 초기화
        /// </summary>
        public abstract void Initialize();
    }
}