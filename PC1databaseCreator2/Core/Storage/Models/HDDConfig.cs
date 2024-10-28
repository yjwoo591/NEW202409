using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using PC1databaseCreator.Common.Library.Core.Models;
using PC1databaseCreator.Common.Library.Core.Storage.Models;

namespace PC1databaseCreator.Core.Storage.Models
{
    /// <summary>
    /// HDD 스토리지 설정 관리 클래스
    /// </summary>
    public class HDDConfig : StorageConfigBase
    {
        /// <summary>
        /// 주 드라이브 목록
        /// </summary>
        public List<StorageDriveInfo> PrimaryDrives { get; private set; }

        /// <summary>
        /// 미러 드라이브 목록
        /// </summary>
        public List<StorageDriveInfo> MirrorDrives { get; private set; }

        public HDDConfig(IConfiguration configuration)
            : base(configuration, StorageType.HDD)
        {
            PrimaryDrives = new List<StorageDriveInfo>();
            MirrorDrives = new List<StorageDriveInfo>();
            StorageId = "HDD_Storage";
        }

        /// <summary>
        /// 드라이브 쌍 추가
        /// </summary>
        public void AddDrivePair(string primaryPath, string mirrorPath)
        {
            ValidatePath(primaryPath, "Primary");
            ValidatePath(mirrorPath, "Mirror");

            var primaryDrive = new StorageDriveInfo(primaryPath)
            {
                DriveNumber = PrimaryDrives.Count
            };
            var mirrorDrive = new StorageDriveInfo(mirrorPath)
            {
                DriveNumber = MirrorDrives.Count
            };

            PrimaryDrives.Add(primaryDrive);
            MirrorDrives.Add(mirrorDrive);

            // 드라이브 설정 저장
            SetSetting($"PrimaryDrive_{PrimaryDrives.Count}", primaryPath);
            SetSetting($"MirrorDrive_{MirrorDrives.Count}", mirrorPath);
        }

        public override void Initialize()
        {
            // 기본 설정 로드
            SetSetting("FastAccessAreaPercentage",
                Configuration.GetValue<int>("Storage:HDD:FastAccessAreaPercentage", 20));

            SetSetting("CacheSizeMB",
                Configuration.GetValue<int>("Storage:HDD:CacheSizeMB", 1024));

            // 드라이브 쌍 로드
            var primaryPaths = Configuration.GetSection("Storage:HDD:PrimaryPaths").Get<string[]>();
            var mirrorPaths = Configuration.GetSection("Storage:HDD:MirrorPaths").Get<string[]>();

            if (primaryPaths?.Length > 0 && primaryPaths.Length == mirrorPaths?.Length)
            {
                for (int i = 0; i < primaryPaths.Length; i++)
                {
                    try
                    {
                        AddDrivePair(primaryPaths[i], mirrorPaths[i]);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"드라이브 쌍 추가 실패: {ex.Message}");
                    }
                }
            }
        }

        public override bool Validate()
        {
            if (PrimaryDrives.Count == 0 || PrimaryDrives.Count != MirrorDrives.Count)
                return false;

            var fastAccessPercentage = GetSetting<int>("FastAccessAreaPercentage");
            if (fastAccessPercentage < 0 || fastAccessPercentage > 100)
                return false;

            var cacheSizeMB = GetSetting<int>("CacheSizeMB");
            if (cacheSizeMB <= 0)
                return false;

            return true;
        }

        private void ValidatePath(string path, string driveType)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path),
                    $"{driveType} drive path cannot be null or empty");

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException(
                    $"{driveType} drive path does not exist: {path}");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                PrimaryDrives.Clear();
                MirrorDrives.Clear();
            }
            base.Dispose(disposing);
        }
    }
}