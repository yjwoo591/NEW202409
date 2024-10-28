using System;

namespace PC1databaseCreator.Core.Storage.Common.Models
{
    /// <summary>
    /// 드라이브 사용량 정보
    /// </summary>
    public class DriveUsageInfo
    {
        /// <summary>
        /// 마지막 쓰기 시간
        /// </summary>
        public DateTime LastWrite { get; set; } = DateTime.Now;

        /// <summary>
        /// 쓰기 작업 수
        /// </summary>
        public long WriteCount { get; set; }

        /// <summary>
        /// 사용된 공간
        /// </summary>
        public long UsedSpace { get; set; }

        /// <summary>
        /// 사용 공간 비율
        /// </summary>
        public double UsedSpacePercentage
        {
            get
            {
                try
                {
                    var driveInfo = new System.IO.DriveInfo(System.IO.Path.GetPathRoot(System.IO.Directory.GetCurrentDirectory()));
                    return (double)UsedSpace / driveInfo.TotalSize * 100;
                }
                catch
                {
                    return 0;
                }
            }
        }
    }
}