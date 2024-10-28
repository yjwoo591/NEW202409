using System;
using System.Collections.Generic;

namespace PC1databaseCreator.Common.Library.Core.Storage.Models
{
    /// <summary>
    /// 스토리지 메트릭스 기본 클래스
    /// </summary>
    public class StorageMetrics
    {
        /// <summary>
        /// 전체 용량 (바이트)
        /// </summary>
        public long TotalSpace { get; set; }

        /// <summary>
        /// 사용 가능한 용량 (바이트)
        /// </summary>
        public long AvailableSpace { get; set; }

        /// <summary>
        /// 사용 중인 용량 (바이트)
        /// </summary>
        public long UsedSpace { get; set; }

        /// <summary>
        /// 드라이브별 사용량
        /// </summary>
        public IDictionary<string, DriveMetrics> DriveUsages { get; set; }

        /// <summary>
        /// 마지막 업데이트 시간
        /// </summary>
        public DateTime LastUpdated { get; set; }

        public StorageMetrics()
        {
            DriveUsages = new Dictionary<string, DriveMetrics>();
            LastUpdated = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// 드라이브 메트릭스
    /// </summary>
    public class DriveMetrics
    {
        /// <summary>
        /// 마지막 쓰기 시간
        /// </summary>
        public DateTime LastWrite { get; set; }

        /// <summary>
        /// 쓰기 횟수
        /// </summary>
        public long WriteCount { get; set; }

        /// <summary>
        /// 사용 중인 용량 (바이트)
        /// </summary>
        public long UsedSpace { get; set; }

        /// <summary>
        /// IOPS
        /// </summary>
        public double IOPS { get; set; }

        /// <summary>
        /// 지연 시간 (ms)
        /// </summary>
        public double Latency { get; set; }

        public DriveMetrics()
        {
            LastWrite = DateTime.UtcNow;
        }
    }
}