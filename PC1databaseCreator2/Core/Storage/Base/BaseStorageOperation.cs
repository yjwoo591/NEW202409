using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PC1databaseCreator.Core.Storage.Base.Interfaces;

namespace PC1databaseCreator.Core.Storage.Base
{
    /// <summary>
    /// 모든 스토리지 작업의 기본 구현을 제공하는 추상 클래스
    /// IStorageOperation 인터페이스를 구현하며, 공통 기능을 제공합니다.
    /// </summary>
    public abstract class BaseStorageOperation : IStorageOperation
    {
        /// <summary>
        /// 작업을 식별하는 고유 ID
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// 작업 유형 (Read/Write/Delete)
        /// </summary>
        public abstract StorageOperationType Type { get; }

        /// <summary>
        /// 로거 인스턴스
        /// </summary>
        protected readonly ILogger _logger;

        /// <summary>
        /// 메트릭스 수집기
        /// </summary>
        protected readonly IStorageMetrics _metrics;

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="logger">로깅을 위한 ILogger 인스턴스</param>
        /// <param name="metrics">메트릭스 수집을 위한 IStorageMetrics 인스턴스</param>
        protected BaseStorageOperation(ILogger logger, IStorageMetrics metrics)
        {
            Id = Guid.NewGuid();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        }

        /// <summary>
        /// 작업 실행
        /// 실행 전후로 메트릭스를 수집하고 로깅을 수행합니다.
        /// </summary>
        public async Task<bool> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting storage operation {OperationType} with ID {OperationId}",
                    Type, Id);
                _metrics.RecordStart(Id, Type);

                // 작업 실행 전 유효성 검사
                if (!Validate())
                {
                    _logger.LogError("Validation failed for operation {OperationId}", Id);
                    return false;
                }

                // 실제 작업 수행
                var result = await ExecuteInternalAsync(cancellationToken);

                // 작업 결과 기록
                _metrics.RecordComplete(new StorageOperationResult(Id, result));

                _logger.LogInformation("Completed storage operation {OperationId} with result {Result}",
                    Id, result);

                return result;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error executing storage operation {OperationId}", Id);
                _metrics.RecordError(Id, ex);
                throw;
            }
        }

        /// <summary>
        /// 작업 유효성 검증
        /// 기본적으로 true를 반환하며, 필요한 경우 하위 클래스에서 재정의합니다.
        /// </summary>
        public virtual bool Validate()
        {
            return true;
        }

        /// <summary>
        /// 실제 작업을 수행하는 추상 메서드
        /// 모든 하위 클래스는 이 메서드를 구현해야 합니다.
        /// </summary>
        protected abstract Task<bool> ExecuteInternalAsync(CancellationToken cancellationToken);
    }

    /// <summary>
    /// 기본 작업 결과 구현
    /// </summary>
    internal class StorageOperationResult : IStorageResult
    {
        public Guid OperationId { get; }
        public bool IsSuccess { get; }
        public DateTime StartTime { get; }
        public DateTime EndTime { get; }
        public string ErrorMessage { get; }

        public StorageOperationResult(Guid operationId, bool success)
        {
            OperationId = operationId;
            IsSuccess = success;
            StartTime = DateTime.UtcNow;
            EndTime = DateTime.UtcNow;
            ErrorMessage = null;
        }
    }
}