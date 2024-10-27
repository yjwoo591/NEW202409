using OneOf;

namespace PC1databaseCreator.Common.Infrastructure.Results
{
    /// <summary>
    /// 작업 결과를 나타내는 제네릭 클래스
    /// </summary>
    public class Result<T> : OneOfBase<T, Error>
    {
        private Result(OneOf<T, Error> input) : base(input) { }

        public static Result<T> Success(T value) => new(value);
        public static Result<T> Failure(string message) => new(new Error(message));
        public static Result<T> Failure(Exception ex) => new(new Error(ex.Message));

        public bool IsSuccess => IsT0;
        public bool IsFailure => IsT1;

        public T Value => IsT0 ? AsT0 : throw new InvalidOperationException("Result is failure");
        public Error Error => IsT1 ? AsT1 : throw new InvalidOperationException("Result is success");
    }

    public record Error(string Message);
}