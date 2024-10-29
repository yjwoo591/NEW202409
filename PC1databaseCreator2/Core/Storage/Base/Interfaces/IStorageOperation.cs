using System;
using System.Threading;
using System.Threading.Tasks;

namespace PC1databaseCreator.Core.Storage.Base.Interfaces
{
    /// <summary>
    /// 스토리지 작업의 기본 인터페이스
    /// 모든 스토리지 작업(읽기,쓰기,삭제)의 기본 형태를 정의합니다.
    /// </summary>
    public interface IStorageOperation
    {
        /// <summary>
        /// 작업 ID - 각 스토리지 작업을 고유하게 식별하기 위한 값
        /// 로깅이나 추적에 사용됩니다.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// 작업 유형 - Read/Write/Delete 중 하나
        /// 작업의 종류를 구분하여 적절한 처리를 하기 위해 사용됩니다.
        /// </summary>
        StorageOperationType Type { get; }

        /// <summary>
        /// 작업 실행 메서드
        /// 실제 스토리지 작업을 수행하며, 비동기로 처리됩니다.
        /// CancellationToken을 통해 작업 취소가 가능합니다.
        /// </summary>
        /// <param name="cancellationToken">작업 취소를 위한 토큰</param>
        /// <returns>작업 성공 여부를 나타내는 bool 값</returns>
        Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 작업 검증 메서드
        /// 실제 작업 수행 전에 필요한 조건들이 충족되었는지 검사합니다.
        /// 예: 파일 존재 여부, 권한 확인 등
        /// </summary>
        /// <returns>검증 성공 여부를 나타내는 bool 값</returns>
        bool Validate();
    }

    /// <summary>
    /// 스토리지 작업 유형을 정의하는 열거형
    /// - Read: 파일 읽기 작업
    /// - Write: 파일 쓰기 작업
    /// - Delete: 파일 삭제 작업
    /// </summary>
    public enum StorageOperationType
    {
        Read,   // 파일 읽기 작업
        Write,  // 파일 쓰기 작업
        Delete  // 파일 삭제 작업
    }
}