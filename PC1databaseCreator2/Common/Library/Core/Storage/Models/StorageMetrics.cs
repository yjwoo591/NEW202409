using System;
using System.Collections.Generic;
using System.IO.Abstractions;

namespace PC1databaseCreator.Common.Library.Core.Storage.Models
{
    /// <summary>
    /// 스토리지 성능 메트릭 클래스
    /// </summary>
    public class StorageMetrics
    {
        /// <summary>
        /// 드라이브별 메트릭
        /// </summary>
        public Dictionary<string, DriveMetrics> DriveMetrics { get; private set; }

        /// <summary>
        /// 마지막 업데이트 시간
        /// </summary>
        public DateTime LastUpdateTime { get; private set; }

        /// <summary>
        /// 생성자
        /// </summary>
        public StorageMetrics()
        {
            DriveMetrics = new Dictionary<string, DriveMetrics>();
            LastUpdateTime = DateTime.Now;
        }

        /// <summary>
        /// 메트릭 업데이트
        /// </summary>
        public void UpdateMetrics(string drivePath, IFileSystem fileSystem)
        {
            if (!DriveMetrics.ContainsKey(drivePath))
            {
                DriveMetrics[drivePath] = new DriveMetrics();
            }

            DriveMetrics[drivePath].Update(fileSystem, drivePath);
            LastUpdateTime = DateTime.Now;
        }
    }

    /// <summary>
    /// 드라이브별 성능 메트릭 클래스
    /// </summary>
    public class DriveMetrics
    {
        /// <summary>
        /// 총 용량 (바이트)
        /// </summary>
        public long TotalSpace { get; private set; }

        /// <summary>
        /// 사용 가능한 용량 (바이트)
        /// </summary>
        public long AvailableSpace { get; private set; }

        /// <summary>
        /// 사용된 용량 (바이트)
        /// </summary>
        public long UsedSpace { get; private set; }

        /// <summary>
        /// 읽기 작업 수
        /// </summary>
        public long ReadOperations { get; private set; }

        /// <summary>
        /// 쓰기 작업 수
        /// </summary>
        public long WriteOperations { get; private set; }

        /// <summary>
        /// IOPS (초당 입출력 작업 수)
        /// </summary>
        public double IOPS { get; private set; }

        /// <summary>
        /// 지연 시간 (밀리초)
        /// </summary>
        public double Latency { get; private set; }

        /// <summary>
        /// 마지막 업데이트 시간
        /// </summary>
        public DateTime LastUpdateTime { get; private set; }

        /// <summary>
        /// 메트릭 업데이트
        /// </summary>
        public void Update(IFileSystem fileSystem, string drivePath)
        {
            var driveInfo = fileSystem.DriveInfo.FromDriveName(fileSystem.Path.GetPathRoot(drivePath));

            TotalSpace = driveInfo.TotalSize;
            AvailableSpace = driveInfo.AvailableFreeSpace;
            UsedSpace = TotalSpace - AvailableSpace;

            // 성능 카운터를 통한 IOPS 및 지연 시간 계산은 별도 구현 필요
            // 현재는 샘플 값으로 설정
            ReadOperations++;
            WriteOperations++;
            IOPS = CalculateIOPS();
            Latency = CalculateLatency();

            LastUpdateTime = DateTime.Now;
        }

        private double CalculateIOPS()
        {
            // 실제 IOPS 계산 로직 구현 필요
            return (ReadOperations + WriteOperations) / 1000.0;
        }

        private double CalculateLatency()
        {
            // 실제 지연 시간 계산 로직 구현 필요
            return 0.5; // 샘플 값 (밀리초)
        }
    }
}