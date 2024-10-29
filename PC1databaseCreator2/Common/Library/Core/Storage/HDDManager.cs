using System;
using System.Threading.Tasks;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using ErrorOr;
using PC1databaseCreator.Common.Library.Core.Storage.Models;

namespace PC1databaseCreator.Common.Library.Core.Storage
{
    /// <summary>
    /// 기본 스토리지 관리자 구현 클래스
    /// </summary>
    public class StorageManager : AbstractStorageManager
    {
        #region Fields
        private readonly IFileSystem _fileSystem;
        private readonly ILogger<StorageManager> _logger;
        private readonly StorageMetrics _metrics;
        #endregion

        #region Constructor
        /// <summary>
        /// StorageManager 생성자
        /// </summary>
        public StorageManager(
            IFileSystem fileSystem,
            ILogger<StorageManager> logger,
            StorageMetrics metrics)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 데이터 저장
        /// </summary>
        public override async Task<ErrorOr<Success>> SaveDataAsync(string path, byte[] data)
        {
            try
            {
                _logger.LogInformation("Saving data to path: {path}, Size: {size} bytes", path, data.Length);

                string directory = _fileSystem.Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !_fileSystem.Directory.Exists(directory))
                {
                    _fileSystem.Directory.CreateDirectory(directory);
                }

                await _fileSystem.File.WriteAllBytesAsync(path, data);
                UpdateMetrics(path);

                _logger.LogInformation("Data saved successfully to: {path}", path);
                return Result.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save data to: {path}", path);
                return Error.Failure("SaveData.Failed", ex.Message);
            }
        }

        /// <summary>
        /// 데이터 로드
        /// </summary>
        public override async Task<ErrorOr<byte[]>> LoadDataAsync(string path)
        {
            try
            {
                _logger.LogInformation("Loading data from path: {path}", path);

                if (!_fileSystem.File.Exists(path))
                {
                    _logger.LogWarning("File not found: {path}", path);
                    return Error.NotFound("LoadData.FileNotFound", $"File not found: {path}");
                }

                var data = await _fileSystem.File.ReadAllBytesAsync(path);
                UpdateMetrics(path);

                _logger.LogInformation("Data loaded successfully from: {path}", path);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load data from: {path}", path);
                return Error.Failure("LoadData.Failed", ex.Message);
            }
        }

        /// <summary>
        /// 데이터 삭제
        /// </summary>
        public override ErrorOr<Success> DeleteData(string path)
        {
            try
            {
                _logger.LogInformation("Deleting file: {path}", path);

                if (!_fileSystem.File.Exists(path))
                {
                    _logger.LogWarning("File not found for deletion: {path}", path);
                    return Error.NotFound("DeleteData.FileNotFound", $"File not found: {path}");
                }

                _fileSystem.File.Delete(path);
                _logger.LogInformation("File deleted successfully: {path}", path);
                return Result.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file: {path}", path);
                return Error.Failure("DeleteData.Failed", ex.Message);
            }
        }

        /// <summary>
        /// 데이터 존재 여부 확인
        /// </summary>
        public override bool Exists(string path)
        {
            return _fileSystem.File.Exists(path);
        }

        /// <summary>
        /// 스토리지 초기화
        /// </summary>
        public override void Initialize()
        {
            _logger.LogInformation("Initializing storage manager");
            // 추가적인 초기화 로직이 필요한 경우 구현
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 메트릭 업데이트
        /// </summary>
        private void UpdateMetrics(string path)
        {
            try
            {
                _metrics.UpdateMetrics(path, _fileSystem);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update metrics for path: {path}", path);
            }
        }
        #endregion
    }
}