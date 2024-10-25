using System;

namespace PC1databaseCreator.Core.Storage.Common
{
    /// <summary>
    /// 드라이브 성능 메트릭 정보
    /// </summary>
    public class DriveMetrics
    {
        /// <summary>
        /// 드라이브 경로
        /// </summary>
        public string DrivePath { get; set; }

        /// <summary>
        /// 초당 I/O 작업 수
        /// </summary>
        public double IOPS { get; set; }

        /// <summary>
        /// 작업 지연시간 (밀리초)
        /// </summary>
        public double Latency { get; set; }

        /// <summary>
        /// 처리량 (MB/s)
        /// </summary>
        public double ThroughputMBps { get; set; }

        /// <summary>
        /// 측정 시간
        /// </summary>
        public DateTime MeasurementTime { get; set; }

        /// <summary>
        /// 측정 성공 여부
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 성능이 허용 범위 내인지 확인
        /// </summary>
        public bool IsPerformanceAcceptable()
        {
            return IOPS >= StorageConstants.MIN_ACCEPTABLE_IOPS &&
                   Latency <= StorageConstants.MAX_ACCEPTABLE_LATENCY_MS &&
                   ThroughputMBps >= StorageConstants.MIN_THROUGHPUT_MBPS;
        }

        /// <summary>
        /// 성능 지표를 문자열로 반환
        /// </summary>
        public override string ToString()
        {
            return $"Drive: {DrivePath}, IOPS: {IOPS:F2}, Latency: {Latency:F2}ms, Throughput: {ThroughputMBps:F2}MB/s";
        }
    }

    /// <summary>
    /// 메트릭 수집 결과
    /// </summary>
    public class MetricsResult
    {
        /// <summary>
        /// 메트릭 데이터
        /// </summary>
        public DriveMetrics Metrics { get; set; }

        /// <summary>
        /// 수집 시작 시간
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 수집 종료 시간
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 수집 성공 여부
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 오류 메시지
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 수집 소요 시간
        /// </summary>
        public TimeSpan Duration => EndTime - StartTime;
    }
}