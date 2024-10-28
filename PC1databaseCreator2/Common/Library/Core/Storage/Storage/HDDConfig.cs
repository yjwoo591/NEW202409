using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using Microsoft.Extensions.Configuration;
using ErrorOr;
using FluentValidation;
using Microsoft.Extensions.Logging;
using PC1databaseCreator.Common.Library.Core.Storage.Models;
using System.Collections.Concurrent;
using System.Linq;

namespace PC1databaseCreator.Common.Library.Core.Storage
{
    /// <summary>
    /// HDD 스토리지 설정 클래스
    /// </summary>
    public class HDDConfig : StorageConfigBase
    {
        #region Constants
        protected const string FAST_ACCESS_FOLDER = "FastAccess";
        protected const string ARCHIVE_FOLDER = "Archive";
        protected const int MIN_FREE_SPACE_GB = 50;
        #endregion

        #region Fields
        private readonly IFileSystem _fileSystem;
        private readonly ILogger<HDDConfig> _logger;
        private readonly HDDConfigValidator _validator;
        #endregion

        #region Properties
        /// <summary>
        /// 주 드라이브 목록
        /// </summary>
        public List<string> PrimaryDrives { get; private set; }

        /// <summary>
        /// 미러 드라이브 목록
        /// </summary>
        public List<string> MirrorDrives { get; private set; }

        /// <summary>
        /// 드라이브 사용 정보
        /// </summary>
        public ConcurrentDictionary<string, DriveUsageInfo> DriveUsage { get; private set; }

        /// <summary>
        /// 현재 드라이브 인덱스
        /// </summary>
        public int CurrentDriveIndex { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// HDDConfig 생성자
        /// </summary>
        public HDDConfig(IConfiguration configuration, IFileSystem fileSystem, ILogger<HDDConfig> logger)
            : base(configuration, StorageType.HDD)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validator = new HDDConfigValidator(_fileSystem);

            PrimaryDrives = new List<string>();
            MirrorDrives = new List<string>();
            DriveUsage = new ConcurrentDictionary<string, DriveUsageInfo>();
            CurrentDriveIndex = 0;

            _logger.LogInformation("HDDConfig initialized with default values");
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 설정 유효성 검사 (StorageConfigBase의 추상 메서드 구현)
        /// </summary>
        public override bool Validate()
        {
            try
            {
                _logger.LogInformation("Validating HDD configuration");
                var validationResult = _validator.Validate(this);

                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        _logger.LogError("Validation error: {error}", error.ErrorMessage);
                    }
                    return false;
                }

                _logger.LogInformation("HDD configuration validation successful");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during configuration validation");
                return false;
            }
        }

        /// <summary>
        /// 스토리지 초기화
        /// </summary>
        public override void Initialize()
        {
            try
            {
                _logger.LogInformation("Initializing HDD storage configuration...");

                // 기본 드라이브 설정
                if (PrimaryDrives.Count == 0)
                {
                    string defaultPath = _fileSystem.Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                        "PC1databaseCreator", "Storage", "HDD");

                    _logger.LogInformation("Setting up default drive paths at: {path}", defaultPath);

                    AddDrivePair(
                        _fileSystem.Path.Combine(defaultPath, "Primary"),
                        _fileSystem.Path.Combine(defaultPath, "Mirror")
                    );
                }

                // 드라이브 초기화
                foreach (var drivePath in PrimaryDrives.Concat(MirrorDrives))
                {
                    InitializeDrive(drivePath);
                }

                _logger.LogInformation("HDD storage configuration initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing HDD storage configuration");
                throw;
            }
        }

        /// <summary>
        /// 드라이브 쌍 추가
        /// </summary>
        public ErrorOr<Success> AddDrivePair(string primaryPath, string mirrorPath)
        {
            try
            {
                _logger.LogInformation("Adding drive pair - Primary: {primaryPath}, Mirror: {mirrorPath}",
                    primaryPath, mirrorPath);

                if (string.IsNullOrEmpty(primaryPath))
                    return Error.Validation("PrimaryPath.Empty", "Primary drive path cannot be empty");

                if (string.IsNullOrEmpty(mirrorPath))
                    return Error.Validation("MirrorPath.Empty", "Mirror drive path cannot be empty");

                PrimaryDrives.Add(primaryPath);
                MirrorDrives.Add(mirrorPath);
                DriveUsage[primaryPath] = new DriveUsageInfo();
                DriveUsage[mirrorPath] = new DriveUsageInfo();

                _logger.LogInformation("Drive pair added successfully");
                return Result.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding drive pair");
                return Error.Failure("AddDrivePair.Failed", ex.Message);
            }
        }
        #endregion

        #region Private Methods
        private void InitializeDrive(string drivePath)
        {
            // FastAccess 폴더 생성
            string fastAccessPath = _fileSystem.Path.Combine(drivePath, FAST_ACCESS_FOLDER);
            if (!_fileSystem.Directory.Exists(fastAccessPath))
            {
                _fileSystem.Directory.CreateDirectory(fastAccessPath);
            }

            // Archive 폴더 생성
            string archivePath = _fileSystem.Path.Combine(drivePath, ARCHIVE_FOLDER);
            if (!_fileSystem.Directory.Exists(archivePath))
            {
                _fileSystem.Directory.CreateDirectory(archivePath);
            }

            // 드라이브 사용 정보 초기화
            DriveUsage.TryAdd(drivePath, new DriveUsageInfo());
        }
        #endregion
    }
}