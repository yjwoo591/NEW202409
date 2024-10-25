using System;

namespace PC1databaseCreator.Core.Storage.Common.Models
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
    }
}