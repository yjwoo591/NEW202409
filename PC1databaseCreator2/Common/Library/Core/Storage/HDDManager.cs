using System;
using System.Threading.Tasks;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using ErrorOr;
using PC1databaseCreator.Common.Library.Core.Storage.Models;

namespace PC1databaseCreator.Common.Library.Core.Storage
{
    /// <summary>
    /// �⺻ ���丮�� ������ ���� Ŭ����
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
        /// StorageManager ������
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
        /// ������ ����
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
        /// ������ �ε�
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
        /// ������ ����
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
        /// ������ ���� ���� Ȯ��
        /// </summary>
        public override bool Exists(string path)
        {
            return _fileSystem.File.Exists(path);
        }

        /// <summary>
        /// ���丮�� �ʱ�ȭ
        /// </summary>
        public override void Initialize()
        {
            _logger.LogInformation("Initializing storage manager");
            // �߰����� �ʱ�ȭ ������ �ʿ��� ��� ����
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// ��Ʈ�� ������Ʈ
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