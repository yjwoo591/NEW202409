using System;
using System.Collections.Generic;
using PC1databaseCreator.Common.Library.Core.Storage.Models;

namespace PC1databaseCreator.Core.Storage.Models
{
    /// <summary>
    /// 스토리지 메트릭스 정보
    /// </summary>
    public class StorageMetrics
    {
        /// <summary>
        /// 총 용량 (바이트)
        /// </summary>
        public long TotalSpace { get; set; }

        /// <summary>
        /// 사용 가능한 용량 (바이트)
        /// </summary>
        public long AvailableSpace { get; set; }

        /// <summary>
        /// 사용중인 용량 (바이트)
        /// </summary>
        public long UsedSpace => TotalSpace - AvailableSpace;

        /// <summary>
        /// 드라이브별 사용량
        /// </summary>
        public IDictionary<string, DriveMetrics> DriveUsages { get; set; }

        /// <summary>
        /// 마지막 업데이트 시간
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// CPU 사용률 (%)
        /// </summary>
        public double CpuUsage { get; set; }

        /// <summary>
        /// 메모리 사용량 (바이트)
        /// </summary>
        public long MemoryUsage { get; set; }

        /// <summary>
        /// IOPS
        /// </summary>
        public int IOPS { get; set; }

        /// <summary>
        /// 지연시간 (밀리초)
        /// </summary>
        public double Latency { get; set; }

        /// <summary>
        /// 처리량 (MB/s)
        /// </summary>
        public double Throughput { get; set; }

        /// <summary>
        /// 생성자
        /// </summary>
        public StorageMetrics()
        {
            DriveUsages = new Dictionary<string, DriveMetrics>();
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// 메트릭스 갱신
        /// </summary>
        public void Update()
        {
            LastUpdated = DateTime.UtcNow;
            foreach (var usage in DriveUsages.Values)
            {
                usage.Update();
            }
        }

        /// <summary>
        /// 사용률 계산 (%)
        /// </summary>
        public double GetUsagePercentage()
        {
            return TotalSpace > 0 ? (double)UsedSpace / TotalSpace * 100 : 0;
        }

        /// <summary>
        /// 문자열 표현
        /// </summary>
        public override string ToString()
        {
            return $"[{LastUpdated:yyyy-MM-dd HH:mm:ss}] Used: {GetUsagePercentage():F1}%, IOPS: {IOPS}, Latency: {Latency:F2}ms";
        }
    }
}