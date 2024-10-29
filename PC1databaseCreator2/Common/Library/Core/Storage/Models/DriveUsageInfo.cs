using System;
using System.IO.Abstractions;

namespace PC1databaseCreator.Common.Library.Core.Storage.Models
{
    /// <summary>
    /// 드라이브 사용량 정보 클래스
    /// </summary>
    public class DriveUsageInfo
    {
        /// <summary>
        /// 마지막 쓰기 시간
        /// </summary>
        public DateTime LastWrite { get; set; } = DateTime.Now;

        /// <summary>
        /// 쓰기 횟수
        /// </summary>
        public long WriteCount { get; set; }

        /// <summary>
        /// 사용된 공간 (바이트)
        /// </summary>
        public long UsedSpace { get; set; }

        /// <summary>
        /// 사용 공간 비율 계산
        /// </summary>
        public double UsedSpacePercentage(IFileSystem fileSystem, string drivePath)
        {
            try
            {
                var root = fileSystem.Path.GetPathRoot(drivePath);
                var driveInfo = fileSystem.DriveInfo.FromDriveName(root);
                return (double)UsedSpace / driveInfo.TotalSize * 100;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}