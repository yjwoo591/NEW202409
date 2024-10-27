using PC1databaseCreator.Common.Infrastructure.Interfaces.Core;

namespace PC1databaseCreator.Common.Results
{
    public class Result : IResult
    {
        public bool IsSuccess { get; }
        public IEnumerable<string> Messages { get; }
        public Exception? Exception { get; }

        protected Result(bool isSuccess, IEnumerable<string> messages, Exception? exception = null)
        {
            IsSuccess = isSuccess;
            Messages = messages.ToList().AsReadOnly();
            Exception = exception;
        }

        public static Result Success()
            => new(true, Array.Empty<string>());

        public static Result Success(string message)
            => new(true, new[] { message });

        public static Result Success(IEnumerable<string> messages)
            => new(true, messages);

        public static Result Failure(string message)
            => new(false, new[] { message });

        public static Result Failure(IEnumerable<string> messages)
            => new(false, messages);

        public static Result Failure(Exception exception)
            => new(false, new[] { exception.Message }, exception);

        public static Result Failure(string message, Exception exception)
            => new(false, new[] { message }, exception);

        public static Result Failure(IEnumerable<string> messages, Exception exception)
            => new(false, messages, exception);

        public Result Ensure(Func<bool> predicate, string errorMessage)
        {
            if (!IsSuccess) return this;
            return predicate() ? this : Failure(errorMessage);
        }

        public async Task<Result> EnsureAsync(Func<Task<bool>> predicate, string errorMessage)
        {
            if (!IsSuccess) return this;
            return await predicate() ? this : Failure(errorMessage);
        }

        public Result Match(Action onSuccess, Action onFailure)
        {
            if (IsSuccess)
                onSuccess();
            else
                onFailure();

            return this;
        }

        public T Match<T>(Func<T> onSuccess, Func<T> onFailure)
        {
            return IsSuccess ? onSuccess() : onFailure();
        }
    }

    public class Result<T> : Result
    {
        public T? Value { get; }

        protected Result(T? value, bool isSuccess, IEnumerable<string> messages, Exception? exception = null)
            : base(isSuccess, messages, exception)
        {
            Value = value;
        }

        public static Result<T> Success(T value)
            => new(value, true, Array.Empty<string>());

        public static Result<T> Success(T value, string message)
            => new(value, true, new[] { message });

        public static Result<T> Success(T value, IEnumerable<string> messages)
            => new(value, true, messages);

        public static new Result<T> Failure(string message)
            => new(default, false, new[] { message });

        public static new Result<T> Failure(IEnumerable<string> messages)
            => new(default, false, messages);

        public static new Result<T> Failure(Exception exception)
            => new(default, false, new[] { exception.Message }, exception);

        public static new Result<T> Failure(string message, Exception exception)
            => new(default, false, new[] { message }, exception);

        public static new Result<T> Failure(IEnumerable<string> messages, Exception exception)
            => new(default, false, messages, exception);

        public Result<TNew> Map<TNew>(Func<T?, TNew> mapper)
        {
            if (!IsSuccess)
                return Result<TNew>.Failure(Messages, Exception);

            try
            {
                return Result<TNew>.Success(mapper(Value));
            }
            catch (Exception ex)
            {
                return Result<TNew>.Failure("Mapping operation failed", ex);
            }
        }

        public async Task<Result<TNew>> MapAsync<TNew>(Func<T?, Task<TNew>> mapper)
        {
            if (!IsSuccess)
                return Result<TNew>.Failure(Messages, Exception);

            try
            {
                var result = await mapper(Value);
                return Result<TNew>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<TNew>.Failure("Async mapping operation failed", ex);
            }
        }

        public Result<T> Ensure(Func<T?, bool> predicate, string errorMessage)
        {
            if (!IsSuccess) return this;
            return predicate(Value) ? this : Failure(errorMessage);
        }

        public async Task<Result<T>> EnsureAsync(Func<T?, Task<bool>> predicate, string errorMessage)
        {
            if (!IsSuccess) return this;
            return await predicate(Value) ? this : Failure(errorMessage);
        }

        public Result<T> OnSuccess(Action<T?> action)
        {
            if (IsSuccess)
                action(Value);
            return this;
        }

        public async Task<Result<T>> OnSuccessAsync(Func<T?, Task> action)
        {
            if (IsSuccess)
                await action(Value);
            return this;
        }

        public Result<T> OnFailure(Action<IEnumerable<string>> action)
        {
            if (!IsSuccess)
                action(Messages);
            return this;
        }

        public TResult Match<TResult>(Func<T?, TResult> onSuccess, Func<IEnumerable<string>, TResult> onFailure)
        {
            return IsSuccess ? onSuccess(Value) : onFailure(Messages);
        }

        public static implicit operator Result<T>(T value)
            => Success(value);

        public static implicit operator T?(Result<T> result)
            => result.Value;
    }
}