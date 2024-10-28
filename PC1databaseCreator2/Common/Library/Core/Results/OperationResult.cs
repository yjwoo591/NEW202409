using System;
using System.Collections.Generic;

namespace PC1databaseCreator.Common.Library.Core.Results
{
    /// <summary>
    /// 작업 결과 인터페이스
    /// </summary>
    public interface IOperationResult
    {
        bool IsSuccess { get; }
        string Message { get; }
        Exception Error { get; }
        IDictionary<string, object> Metadata { get; }
    }

    /// <summary>
    /// 제네릭 작업 결과 인터페이스
    /// </summary>
    public interface IOperationResult<T> : IOperationResult
    {
        T Data { get; }
    }

    /// <summary>
    /// 기본 작업 결과 구현
    /// </summary>
    public class OperationResult : IOperationResult
    {
        public bool IsSuccess { get; protected set; }
        public string Message { get; protected set; }
        public Exception Error { get; protected set; }
        public IDictionary<string, object> Metadata { get; }

        protected OperationResult()
        {
            Metadata = new Dictionary<string, object>();
        }

        public static OperationResult Success(string message = null)
        {
            return new OperationResult
            {
                IsSuccess = true,
                Message = message
            };
        }

        public static OperationResult Failure(string message, Exception error = null)
        {
            return new OperationResult
            {
                IsSuccess = false,
                Message = message,
                Error = error
            };
        }
    }

    /// <summary>
    /// 제네릭 작업 결과 구현
    /// </summary>
    public class OperationResult<T> : OperationResult, IOperationResult<T>
    {
        public T Data { get; protected set; }

        public static OperationResult<T> Success(T data, string message = null)
        {
            return new OperationResult<T>
            {
                IsSuccess = true,
                Message = message,
                Data = data
            };
        }

        public new static OperationResult<T> Failure(string message, Exception error = null)
        {
            return new OperationResult<T>
            {
                IsSuccess = false,
                Message = message,
                Error = error
            };
        }
    }
}