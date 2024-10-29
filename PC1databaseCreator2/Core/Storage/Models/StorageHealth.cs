using System;
using PC1databaseCreator.Common.Infrastructure.Enums;  // Common의 StorageType 사용

namespace PC1databaseCreator.Core.Storage.Models
{
    /// <summary>
    /// 스토리지 상태 정보
    /// </summary>
    public class StorageHealth
    {
        /// <summary>
        /// 스토리지 유형
        /// </summary>
        public StorageType StorageType { get; }

        /// <summary>
        /// 스토리지 ID
        /// </summary>
        public string StorageId { get; }

        /// <summary>
        /// 상태 확인 시간
        /// </summary>
        public DateTime CheckTime { get; }

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
        /// 사용률 (%)
        /// </summary>
        public double UsagePercentage => TotalSpace > 0 ? (double)UsedSpace / TotalSpace * 100 : 0;

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
        /// 상태
        /// </summary>
        public StorageStatus Status { get; set; }

        /// <summary>
        /// 마지막 오류 메시지
        /// </summary>
        public string LastErrorMessage { get; set; }

        /// <summary>
        /// 생성자
        /// </summary>
        public StorageHealth(StorageType storageType, string storageId)
        {
            StorageType = storageType;
            StorageId = storageId ?? throw new ArgumentNullException(nameof(storageId));
            CheckTime = DateTime.UtcNow;
            Status = StorageStatus.Unknown;
        }

        /// <summary>
        /// 상태 검사
        /// </summary>
        public bool IsHealthy()
        {
            if (AvailableSpace < 1024 * 1024 * 1024) // 1GB 미만
                return false;

            if (UsagePercentage > 95)
                return false;

            if (Latency > 100) // 100ms 초과
                return false;

            if (IOPS < 1000)
                return false;

            return Status == StorageStatus.Healthy;
        }

        /// <summary>
        /// 문자열 표현
        /// </summary>
        public override string ToString()
        {
            return $"[{CheckTime:yyyy-MM-dd HH:mm:ss}] {StorageType} - {StorageId}: {Status} ({UsagePercentage:F1}% used)";
        }
    }

    /// <summary>
    /// 스토리지 상태 열거형
    /// </summary>
    public enum StorageStatus
    {
        Unknown,
        Healthy,
        Warning,
        Critical,
        Error
    }
}