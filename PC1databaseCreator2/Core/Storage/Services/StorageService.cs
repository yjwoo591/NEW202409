using Microsoft.Extensions.Logging;
using ErrorOr;
using Polly;
using PC1databaseCreator.Core.Storage.Base.Interfaces;

namespace PC1databaseCreator.Core.Storage.Services
{
    public class StorageService : IStorageService
    {
        private readonly ILogger<StorageService> _logger;
        private readonly string _basePath;
        private readonly IAsyncPolicy<ErrorOr<bool>> _boolRetryPolicy;
        private readonly IAsyncPolicy<ErrorOr<byte[]>> _dataRetryPolicy;

        public StorageService(ILogger<StorageService> logger, string basePath)
        {
            _logger = logger;
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));

            // 데이터 저장/삭제용 정책
            _boolRetryPolicy = Policy<ErrorOr<bool>>
                .Handle<IOException>()
                .Or<UnauthorizedAccessException>()
                .WaitAndRetryAsync(3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, _) =>
                    {
                        _logger.LogWarning(exception.Exception,
                            "Retry {RetryCount} after {Delay}s due to: {Message}",
                            retryCount, timeSpan.TotalSeconds, exception.Exception.Message);
                        return Task.CompletedTask;
                    });

            // 데이터 로드용 정책
            _dataRetryPolicy = Policy<ErrorOr<byte[]>>
                .Handle<IOException>()
                .Or<UnauthorizedAccessException>()
                .WaitAndRetryAsync(3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, _) =>
                    {
                        _logger.LogWarning(exception.Exception,
                            "Retry {RetryCount} after {Delay}s due to: {Message}",
                            retryCount, timeSpan.TotalSeconds, exception.Exception.Message);
                        return Task.CompletedTask;
                    });
        }

        public async Task<ErrorOr<bool>> SaveDataAsync(
            string name,
            byte[] data,
            CancellationToken cancellationToken = default)
        {
            return await _boolRetryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    if (string.IsNullOrEmpty(name))
                        return Error.Validation("Name cannot be empty");

                    if (data is null or { Length: 0 })
                        return Error.Validation("Data cannot be empty");

                    string filePath = Path.Combine(_basePath, name);
                    await using var fileStream = new FileStream(
                        filePath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None,
                        bufferSize: 4096,
                        useAsync: true);

                    await fileStream.WriteAsync(data, cancellationToken);

                    _logger.LogInformation("Successfully saved data: {Name}", name);
                    return true;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Failed to save data: {Name}", name);
                    return Error.Failure(ex.Message);
                }
            });
        }

        public async Task<ErrorOr<byte[]>> LoadDataAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            return await _dataRetryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    if (string.IsNullOrEmpty(name))
                        return Error.Validation("Name cannot be empty");

                    string filePath = Path.Combine(_basePath, name);

                    if (!File.Exists(filePath))
                        return Error.NotFound($"File not found: {name}");

                    await using var fileStream = new FileStream(
                        filePath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read,
                        bufferSize: 4096,
                        useAsync: true);

                    var data = new byte[fileStream.Length];
                    await fileStream.ReadAsync(data.AsMemory(), cancellationToken);

                    _logger.LogInformation("Successfully loaded data: {Name}", name);
                    return data;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Failed to load data: {Name}", name);
                    return Error.Failure(ex.Message);
                }
            });
        }

        public async Task<ErrorOr<bool>> DeleteDataAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            return await _boolRetryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    if (string.IsNullOrEmpty(name))
                        return Error.Validation("Name cannot be empty");

                    string filePath = Path.Combine(_basePath, name);

                    if (!File.Exists(filePath))
                        return Error.NotFound($"File not found: {name}");

                    await Task.Run(() => File.Delete(filePath), cancellationToken);

                    _logger.LogInformation("Successfully deleted data: {Name}", name);
                    return true;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Failed to delete data: {Name}", name);
                    return Error.Failure(ex.Message);
                }
            });
        }
    }
}