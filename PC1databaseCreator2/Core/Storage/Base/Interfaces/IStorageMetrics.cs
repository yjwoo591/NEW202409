using System;
using System.Threading.Tasks;

namespace PC1databaseCreator.Core.Storage.Base.Interfaces
{
    /// <summary>
    /// 스토리지 작업의 성능과 상태를 측정하고 기록하는 인터페이스
    /// 모든 스토리지 작업의 성능 지표를 수집하고 분석하는데 사용됩니다.
    /// </summary>
    public interface IStorageMetrics
    {
        /// <summary>
        /// 작업 시작을 기록
        /// 작업이 시작될 때 호출되어 시작 시간과 작업 정보를 기록합니다.
        /// </summary>
        /// <param name="operationId">작업 ID</param>
        /// <param name="type">작업 유형 (Read/Write/Delete)</param>
        void RecordStart(Guid operationId, StorageOperationType type);

        /// <summary>
        /// 작업 완료를 기록
        /// 작업이 완료될 때 호출되어 결과와 소요 시간을 기록합니다.
        /// </summary>
        /// <param name="result">작업 결과 정보</param>
        void RecordComplete(IStorageResult result);

        /// <summary>
        /// 작업 실패를 기록
        /// 작업 중 오류 발생 시 호출되어 오류 정보를 기록합니다.
        /// </summary>
        /// <param name="operationId">작업 ID</param>
        /// <param name="error">발생한 예외</param>
        void RecordError(Guid operationId, Exception error);

        /// <summary>
        /// 메모리 사용량 기록
        /// 현재 메모리 사용 상태를 기록합니다.
        /// </summary>
        /// <param name="operationId">작업 ID</param>
        /// <param name="bytes">사용된 메모리 크기(바이트)</param>
        void RecordMemoryUsage(Guid operationId, long bytes);

        /// <summary>
        /// 성능 보고서 생성
        /// 지정된 기간 동안의 성능 통계를 생성합니다.
        /// </summary>
        /// <param name="startTime">시작 시간</param>
        /// <param name="endTime">종료 시간</param>
        /// <returns>성능 보고서 데이터</returns>
        Task<StorageMetricsReport> GenerateReportAsync(DateTime startTime, DateTime endTime);
    }

    /// <summary>
    /// 성능 보고서 데이터 구조
    /// 특정 기간 동안의 스토리지 작업 성능 통계를 포함합니다.
    /// </summary>
    public class StorageMetricsReport
    {
        /// <summary>
        /// 보고서 대상 기간의 시작 시간
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 보고서 대상 기간의 종료 시간
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 총 작업 수행 횟수
        /// </summary>
        public int TotalOperations { get; set; }

        /// <summary>
        /// 성공한 작업 수
        /// </summary>
        public int SuccessfulOperations { get; set; }

        /// <summary>
        /// 실패한 작업 수
        /// </summary>
        public int FailedOperations { get; set; }

        /// <summary>
        /// 평균 작업 소요 시간 (밀리초)
        /// </summary>
        public double AverageOperationTime { get; set; }

        /// <summary>
        /// 평균 메모리 사용량 (바이트)
        /// </summary>
        public double AverageMemoryUsage { get; set; }

        /// <summary>
        /// 작업 유형별 수행 횟수
        /// </summary>
        public Dictionary<StorageOperationType, int> OperationTypeCounts { get; set; }
            = new Dictionary<StorageOperationType, int>();
    }
}