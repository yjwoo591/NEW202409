```csharp
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PC1databaseCreator.Core.Storage.Base.Interfaces;
using PC1databaseCreator.Core.Storage.Operations;

namespace PC1databaseCreator.Core.Storage
{
    /// <summary>
    /// 스토리지 작업을 관리하고 실행하는 관리자 클래스
    /// </summary>
    public class StorageManager : IDisposable
    {
        private readonly ILogger<StorageManager> _logger;
        private readonly IStorageMetrics _metrics;
        private readonly ConcurrentDictionary<Guid, IStorageOperation> _activeOperations;
        private readonly SemaphoreSlim _operationThrottle;
        private bool _disposed;

        /// <summary>
        /// 동시 실행 가능한 최대 작업 수
        /// </summary>
        private const int MAX_CONCURRENT_OPERATIONS = 5;

        public StorageManager(
            ILogger<StorageManager> logger,
            IStorageMetrics metrics)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
            _activeOperations = new ConcurrentDictionary<Guid, IStorageOperation>();
            _operationThrottle = new SemaphoreSlim(MAX_CONCURRENT_OPERATIONS);
        }

        /// <summary>
        /// 파일 읽기 작업 수행
        /// </summary>
        public async Task<IStorageResult<byte[]>> ReadFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            await _operationThrottle.WaitAsync(cancellationToken);
            try
            {
                var operation = new ReadOperation(filePath, _logger.CreateLogger<ReadOperation>(), _metrics);
                _activeOperations.TryAdd(operation.Id, operation);

                try
                {
                    return await ((ReadOperation)operation).ReadFileAsync(cancellationToken);
                }
                finally
                {
                    _activeOperations.TryRemove(operation.Id, out _);
                }
            }
            finally
            {
                _operationThrottle.Release();
            }
        }

        /// <summary>
        /// 파일 쓰기 작업 수행
        /// </summary>
        public async Task<IStorageResult> WriteFileAsync(
            string filePath,
            byte[] data,
            bool overwriteIfExists = false,
            CancellationToken cancellationToken = default)
        {
            await _operationThrottle.WaitAsync(cancellationToken);
            try
            {
                var operation = new WriteOperation(
                    filePath,
                    data,
                    _logger.CreateLogger<WriteOperation>(),
                    _metrics,
                    overwriteIfExists: overwriteIfExists);

                _activeOperations.TryAdd(operation.Id, operation);

                try
                {
                    var success = await operation.ExecuteAsync(cancellationToken);
                    return new StorageOperationResult(operation.Id, success);
                }
                finally
                {
                    _activeOperations.TryRemove(operation.Id, out _);
                }
            }
            finally
            {
                _operationThrottle.Release();
            }
        }

        /// <summary>
        /// 파일 삭제 작업 수행
        /// </summary>
        public async Task<IStorageResult> DeleteFileAsync(
            string filePath,
            bool secureDelete = false,
            CancellationToken cancellationToken = default)
        {
            await _operationThrottle.WaitAsync(cancellationToken);
            try
            {
                var operation = new DeleteOperation(
                    filePath,
                    _logger.CreateLogger<DeleteOperation>(),
                    _metrics,
                    ensureComplete: secureDelete);

                _activeOperations.TryAdd(operation.Id, operation);

                try
                {
                    var success = await operation.ExecuteAsync(cancellationToken);
                    return new StorageOperationResult(operation.Id, success);
                }
                finally
                {
                    _activeOperations.TryRemove(operation.Id, out _);
                }
            }
            finally
            {
                _operationThrottle.Release();
            }
        }

        /// <summary>
        /// 현재 진행 중인 작업 취소
        /// </summary>
        public async Task CancelOperationAsync(Guid operationId)
        {
            if (_activeOperations.TryGetValue(operationId, out var operation))
            {
                _logger.LogInformation("Cancelling operation: {OperationId}", operationId);
                // 작업 취소 로직 구현
            }
        }

        /// <summary>
        /// 리소스 정리
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _operationThrottle.Dispose();
            _disposed = true;

            GC.SuppressFinalize(this);
        }

        private class StorageOperationResult : IStorageResult
        {
            public Guid OperationId { get; }
            public bool IsSuccess { get; }
            public DateTime StartTime { get; }
            public DateTime EndTime { get; }
            public string ErrorMessage { get; }

            public StorageOperationResult(Guid operationId, bool success, string errorMessage = null)
            {
                OperationId = operationId;
                IsSuccess = success;
                StartTime = DateTime.UtcNow;
                EndTime = DateTime.UtcNow;
                ErrorMessage = errorMessage;
            }
        }
    }
}
```