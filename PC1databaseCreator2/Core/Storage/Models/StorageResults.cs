using System;

namespace PC1databaseCreator.Core.Storage.Models
{
    // 열거형을 Models 네임스페이스로 이동
    public enum StorageErrorType
    {
        None,
        InsufficientSpace,
        AccessDenied,
        PathNotFound,
        InvalidOperation
    }

    public record StorageResult
    {
        public bool IsSuccess { get; init; }
        public StorageErrorType ErrorType { get; init; }
        public string Message { get; init; }
        public Exception Exception { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;

        public static StorageResult Success(string message = null) =>
            new() { IsSuccess = true, Message = message };

        public static StorageResult Failure(StorageErrorType errorType, string message, Exception ex = null) =>
            new() { IsSuccess = false, ErrorType = errorType, Message = message, Exception = ex };
    }

    public record StorageResult<T> : StorageResult
    {
        public T Data { get; init; }

        public static StorageResult<T> Success(T data, string message = null) =>
            new() { IsSuccess = true, Data = data, Message = message };

        public static new StorageResult<T> Failure(StorageErrorType errorType, string message, Exception ex = null) =>
            new() { IsSuccess = false, ErrorType = errorType, Message = message, Exception = ex };
    }
}