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
    /// 파일 읽기 작업을 수행하는 클래스
    /// </summary>
    public class ReadOperation : BaseStorageOperation
    {
        private readonly string _filePath;
        private readonly long _maxFileSize;

        /// <summary>
        /// 읽기 작업임을 나타내는 작업 유형
        /// </summary>
        public override StorageOperationType Type => StorageOperationType.Read;

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="filePath">읽을 파일의 경로</param>
        /// <param name="maxFileSize">허용되는 최대 파일 크기 (기본값: 100MB)</param>
        /// <param name="logger">로거 인스턴스</param>
        /// <param name="metrics">메트릭스 수집기</param>
        public ReadOperation(
            string filePath,
            ILogger<ReadOperation> logger,
            IStorageMetrics metrics,
            long maxFileSize = 104857600) // 100MB
            : base(logger, metrics)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _maxFileSize = maxFileSize;
        }

        /// <summary>
        /// 파일 읽기 작업 실행 전 유효성 검사
        /// </summary>
        public override bool Validate()
        {
            try
            {
                // 파일 존재 여부 확인
                if (!File.Exists(_filePath))
                {
                    _logger.LogError("File not found: {FilePath}", _filePath);
                    return false;
                }

                // 파일 크기 확인
                var fileInfo = new FileInfo(_filePath);
                if (fileInfo.Length > _maxFileSize)
                {
                    _logger.LogError(
                        "File size ({Size} bytes) exceeds maximum allowed size ({MaxSize} bytes)",
                        fileInfo.Length, _maxFileSize);
                    return false;
                }

                // 파일 접근 권한 확인
                try
                {
                    using var stream = File.OpenRead(_filePath);
                }
                catch (UnauthorizedAccessException)
                {
                    _logger.LogError("Access denied to file: {FilePath}", _filePath);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during read validation for file: {FilePath}", _filePath);
                return false;
            }
        }

        /// <summary>
        /// 실제 파일 읽기 작업 수행
        /// </summary>
        protected override async Task<bool> ExecuteInternalAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var fileStream = new FileStream(
                    _filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 4096,
                    useAsync: true);

                // 파일 크기 기록
                var fileSize = fileStream.Length;
                _metrics.RecordMemoryUsage(Id, fileSize);

                _logger.LogInformation(
                    "Successfully read file: {FilePath}, Size: {Size} bytes",
                    _filePath, fileSize);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file: {FilePath}", _filePath);
                return false;
            }
        }

        /// <summary>
        /// 읽기 작업 결과와 함께 파일 데이터를 반환하는 메서드
        /// </summary>
        /// <returns>파일 데이터와 작업 결과를 포함하는 IStorageResult</returns>
        public async Task<IStorageResult<byte[]>> ReadFileAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var success = await ExecuteAsync(cancellationToken);
                if (!success)
                {
                    return new StorageReadResult(Id, null, "File read operation failed");
                }

                var fileData = await File.ReadAllBytesAsync(_filePath, cancellationToken);
                return new StorageReadResult(Id, fileData);
            }
            catch (Exception ex)
            {
                return new StorageReadResult(Id, null, ex.Message);
            }
        }

        /// <summary>
        /// 파일 읽기 작업의 결과를 나타내는 내부 클래스
        /// </summary>
        private class StorageReadResult : IStorageResult<byte[]>
        {
            public Guid OperationId { get; }
            public bool IsSuccess => ErrorMessage == null;
            public DateTime StartTime { get; }
            public DateTime EndTime { get; }
            public string ErrorMessage { get; }
            public byte[] Data { get; }

            public StorageReadResult(Guid operationId, byte[] data, string errorMessage = null)
            {
                OperationId = operationId;
                StartTime = DateTime.UtcNow;
                Data = data;
                ErrorMessage = errorMessage;
                EndTime = DateTime.UtcNow;
            }
        }
    }
}