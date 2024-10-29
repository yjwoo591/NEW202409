using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PC1databaseCreator.Core.Storage.Base;
using PC1databaseCreator.Core.Storage.Base.Interfaces;

namespace PC1databaseCreator.Core.Storage.Operations
{
    /// <summary>
    /// 파일 쓰기 작업을 수행하는 클래스
    /// </summary>
    public class WriteOperation : BaseStorageOperation
    {
        private readonly string _filePath;
        private readonly byte[] _data;
        private readonly long _maxFileSize;
        private readonly bool _overwriteIfExists;

        /// <summary>
        /// 쓰기 작업임을 나타내는 작업 유형
        /// </summary>
        public override StorageOperationType Type => StorageOperationType.Write;

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="filePath">저장할 파일 경로</param>
        /// <param name="data">저장할 데이터</param>
        /// <param name="logger">로거 인스턴스</param>
        /// <param name="metrics">메트릭스 수집기</param>
        /// <param name="maxFileSize">최대 허용 파일 크기 (기본값: 100MB)</param>
        /// <param name="overwriteIfExists">파일 존재 시 덮어쓰기 여부</param>
        public WriteOperation(
            string filePath,
            byte[] data,
            ILogger<WriteOperation> logger,
            IStorageMetrics metrics,
            long maxFileSize = 104857600, // 100MB
            bool overwriteIfExists = false)
            : base(logger, metrics)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _data = data ?? throw new ArgumentNullException(nameof(data));
            _maxFileSize = maxFileSize;
            _overwriteIfExists = overwriteIfExists;
        }

        /// <summary>
        /// 파일 쓰기 작업 실행 전 유효성 검사
        /// </summary>
        public override bool Validate()
        {
            try
            {
                // 데이터 크기 확인
                if (_data.Length > _maxFileSize)
                {
                    _logger.LogError(
                        "Data size ({Size} bytes) exceeds maximum allowed size ({MaxSize} bytes)",
                        _data.Length, _maxFileSize);
                    return false;
                }

                // 파일 존재 여부 확인
                if (File.Exists(_filePath) && !_overwriteIfExists)
                {
                    _logger.LogError("File already exists and overwrite is not allowed: {FilePath}",
                        _filePath);
                    return false;
                }

                // 디렉토리 존재 여부 확인 및 생성
                var directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    try
                    {
                        Directory.CreateDirectory(directory);
                        _logger.LogInformation("Created directory: {Directory}", directory);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create directory: {Directory}", directory);
                        return false;
                    }
                }

                // 파일 쓰기 권한 확인
                try
                {
                    using var stream = File.Create(_filePath, 1, FileOptions.DeleteOnClose);
                }
                catch (UnauthorizedAccessException)
                {
                    _logger.LogError("Access denied to file path: {FilePath}", _filePath);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during write validation for file: {FilePath}", _filePath);
                return false;
            }
        }

        /// <summary>
        /// 실제 파일 쓰기 작업 수행
        /// </summary>
        protected override async Task<bool> ExecuteInternalAsync(CancellationToken cancellationToken)
        {
            try
            {
                // 임시 파일에 먼저 쓰기
                var tempFilePath = Path.Combine(
                    Path.GetDirectoryName(_filePath),
                    $"{Path.GetFileNameWithoutExtension(_filePath)}.tmp{Path.GetExtension(_filePath)}");

                await using (var fileStream = new FileStream(
                    tempFilePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 4096,
                    useAsync: true))
                {
                    await fileStream.WriteAsync(_data, cancellationToken);
                    await fileStream.FlushAsync(cancellationToken);
                }

                // 임시 파일을 실제 파일로 이동 (atomic operation)
                File.Move(tempFilePath, _filePath, _overwriteIfExists);

                // 메모리 사용량 기록
                _metrics.RecordMemoryUsage(Id, _data.Length);

                _logger.LogInformation(
                    "Successfully wrote file: {FilePath}, Size: {Size} bytes",
                    _filePath, _data.Length);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing file: {FilePath}", _filePath);
                return false;
            }
        }
    }
}