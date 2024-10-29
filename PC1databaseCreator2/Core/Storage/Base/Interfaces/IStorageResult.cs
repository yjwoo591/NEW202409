using System;

namespace PC1databaseCreator.Core.Storage.Base.Interfaces
{
    /// <summary>
    /// 스토리지 작업의 결과를 나타내는 인터페이스
    /// 모든 스토리지 작업(읽기,쓰기,삭제)의 결과를 표준화된 형식으로 반환합니다.
    /// </summary>
    public interface IStorageResult
    {
        /// <summary>
        /// 작업 ID - 어떤 작업의 결과인지 식별하기 위한 고유 값
        /// IStorageOperation의 Id와 매칭됩니다.
        /// </summary>
        Guid OperationId { get; }

        /// <summary>
        /// 작업 성공 여부
        /// true: 작업 성공
        /// false: 작업 실패
        /// </summary>
        bool IsSuccess { get; }

        /// <summary>
        /// 작업 시작 시간
        /// 성능 모니터링 및 로깅에 사용됩니다.
        /// </summary>
        DateTime StartTime { get; }

        /// <summary>
        /// 작업 종료 시간
        /// 작업 소요 시간 계산에 사용됩니다.
        /// </summary>
        DateTime EndTime { get; }

        /// <summary>
        /// 오류 메시지
        /// 작업 실패 시에만 값이 설정됩니다.
        /// 성공 시에는 null입니다.
        /// </summary>
        string ErrorMessage { get; }
    }

    /// <summary>
    /// 제네릭 스토리지 작업 결과 인터페이스
    /// 작업 결과에 구체적인 데이터 타입을 지정할 수 있습니다.
    /// 예: IStorageResult<byte[]> - 파일 읽기 결과
    /// </summary>
    public interface IStorageResult<T> : IStorageResult
    {
        /// <summary>
        /// 작업 결과 데이터
        /// 작업이 성공했을 때만 유효한 값을 가집니다.
        /// 실패 시에는 기본값을 가집니다.
        /// </summary>
        T Data { get; }
    }
}