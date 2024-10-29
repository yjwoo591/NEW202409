using System;
using System.IO;
using System.Threading.Tasks;

namespace PC1databaseCreator.Common.Library.Core.Storage.Models
{
    /// <summary>
    /// 스토리지 드라이브 정보 클래스
    /// </summary>
    public class StorageDriveInfo
    {
        private readonly string _path;
        private System.IO.DriveInfo _systemDriveInfo;

        /// <summary>
        /// 드라이브 경로
        /// </summary>
        public string Path
        {
            get => _path;
            init => _path = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 드라이브 ID
        /// </summary>
        public string DriveId { get; }

        /// <summary>
        /// 드라이브 번호
        /// </summary>
        public int DriveNumber { get; set; }

        /// <summary>
        /// 생성 시간
        /// </summary>
        public DateTime CreatedAt { get; }

        /// <summary>
        /// 마지막 접근 시간
        /// </summary>
        public DateTime LastAccessTime { get; set; }

        /// <summary>
        /// 드라이브 상태
        /// </summary>
        public DriveStatus Status { get; set; }

        /// <summary>
        /// 총 용량 (바이트)
        /// </summary>
        public long TotalSize => _systemDriveInfo.TotalSize;

        /// <summary>
        /// 사용 가능한 용량 (바이트)
        /// </summary>
        public long AvailableFreeSpace => _systemDriveInfo.AvailableFreeSpace;

        /// <summary>
        /// 드라이브 유형
        /// </summary>
        public DriveType DriveType => _systemDriveInfo.DriveType;

        /// <summary>
        /// 드라이브 포맷
        /// </summary>
        public string DriveFormat => _systemDriveInfo.DriveFormat;

        /// <summary>
        /// 볼륨 레이블
        /// </summary>
        public string VolumeLabel
        {
            get => _systemDriveInfo.VolumeLabel;
            set => _systemDriveInfo.VolumeLabel = value;
        }

        /// <summary>
        /// 드라이브가 준비되었는지 여부
        /// </summary>
        public bool IsReady => _systemDriveInfo.IsReady;

        public StorageDriveInfo(string path)
        {
            Path = path;
            DriveId = Guid.NewGuid().ToString();
            CreatedAt = DateTime.UtcNow;
            LastAccessTime = DateTime.UtcNow;
            Status = DriveStatus.Unknown;

            InitializeDriveInfo();
        }

        private void InitializeDriveInfo()
        {
            try
            {
                _systemDriveInfo = new System.IO.DriveInfo(System.IO.Path.GetPathRoot(Path));
                Status = _systemDriveInfo.IsReady ? DriveStatus.Active : DriveStatus.Inactive;
            }
            catch (Exception)
            {
                Status = DriveStatus.Error;
                throw;
            }
        }

        /// <summary>
        /// 드라이브 상태 업데이트
        /// </summary>
        public async Task UpdateStatusAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    // DriveInfo 재생성으로 최신 정보 획득
                    _systemDriveInfo = new System.IO.DriveInfo(System.IO.Path.GetPathRoot(Path));
                    Status = _systemDriveInfo.IsReady ? DriveStatus.Active : DriveStatus.Inactive;
                    LastAccessTime = DateTime.UtcNow;
                });
            }
            catch (Exception)
            {
                Status = DriveStatus.Error;
                throw;
            }
        }

        /// <summary>
        /// 사용 가능한 공간이 충분한지 확인
        /// </summary>
        public bool HasSufficientSpace(long requiredSpace, long minimumFreeSpace)
        {
            // 최신 정보로 DriveInfo 갱신
            _systemDriveInfo = new System.IO.DriveInfo(System.IO.Path.GetPathRoot(Path));
            return AvailableFreeSpace >= (requiredSpace + minimumFreeSpace);
        }

        /// <summary>
        /// 드라이브 정보 갱신
        /// </summary>
        public void UpdateDriveInfo()
        {
            // 새로운 DriveInfo 인스턴스 생성으로 최신 정보 획득
            _systemDriveInfo = new System.IO.DriveInfo(System.IO.Path.GetPathRoot(Path));
        }
    }

    /// <summary>
    /// 드라이브 상태 열거형
    /// </summary>
    public enum DriveStatus
    {
        Unknown = 0,
        Active = 1,
        Inactive = 2,
        Error = 3,
        Maintenance = 4
    }
}