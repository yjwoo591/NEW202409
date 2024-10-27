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
    /// 파일 삭제 작업을 수행하는 클래스
    /// </summary>
    public class DeleteOperation : BaseStorageOperation
    {
        private readonly string _filePath;
        private readonly bool _ensureComplete;
        private long _fileSize;  // 삭제 전 파일 크기 저장용

        /// <summary>
        /// 삭제 작업임을 나타내는 작업 유형
        /// </summary>
        public override StorageOperationType Type => StorageOperationType.Delete;

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="filePath">삭제할 파일 경로</param>
        /// <param name="logger">로거 인스턴스</param>
        /// <param name="metrics">메트릭스 수집기</param>
        /// <param name="ensureComplete">파일이 완전히 삭제되었는지 확인할지 여부</param>
        public DeleteOperation(
            string filePath,
            ILogger<DeleteOperation> logger,
            IStorageMetrics metrics,
            bool ensureComplete = true)
            : base(logger, metrics)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _ensureComplete = ensureComplete;
        }

        /// <summary>
        /// 파일 삭제 작업 실행 전 유효성 검사
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

                // 파일 접근 권한 확인
                try
                {
                    // 파일 크기 저장 (메트릭스용)
                    var fileInfo = new FileInfo(_filePath);
                    _fileSize = fileInfo.Length;

                    // 파일 접근 가능 여부 확인
                    using var stream = File.Open(_filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                }
                catch (UnauthorizedAccessException)
                {
                    _logger.LogError("Access denied to file: {FilePath}", _filePath);
                    return false;
                }
                catch (IOException ex)
                {
                    _logger.LogError(ex, "File is in use: {FilePath}", _filePath);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during delete validation for file: {FilePath}", _filePath);
                return false;
            }
        }

        /// <summary>
        /// 실제 파일 삭제 작업 수행
        /// </summary>
        protected override async Task<bool> ExecuteInternalAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_ensureComplete)
                {
                    // 파일을 0으로 덮어쓰기 (안전한 삭제)
                    await using (var stream = new FileStream(
                        _filePath,
                        FileMode.Open,
                        FileAccess.Write,
                        FileShare.None,
                        4096,
                        useAsync: true))
                    {
                        var zeros = new byte[4096];
                        var remaining = _fileSize;

                        while (remaining > 0)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            var writeSize = (int)Math.Min(remaining, zeros.Length);
                            await stream.WriteAsync(zeros, 0, writeSize, cancellationToken);
                            remaining -= writeSize;
                        }

                        await stream.FlushAsync(cancellationToken);
                    }
                }

                // 파일 삭제
                File.Delete(_filePath);

                // 메모리 사용량 기록 (삭제된 파일 크기)
                _metrics.RecordMemoryUsage(Id, _fileSize);

                _logger.LogInformation(
                    "Successfully deleted file: {FilePath}, Size: {Size} bytes, Secure delete: {SecureDelete}",
                    _filePath, _fileSize, _ensureComplete);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FilePath}", _filePath);
                return false;
            }
        }

        /// <summary>
        /// 삭제가 성공적으로 완료되었는지 확인
        /// </summary>
        protected bool VerifyDeletion()
        {
            return !File.Exists(_filePath);
        }
    }
}번호 순서대로 작업 진행합시다.