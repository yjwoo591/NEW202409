namespace PC1databaseCreator.Common.Infrastructure.Exceptions
{
    /// <summary>
    /// 애플리케이션의 기본 예외 클래스
    /// </summary>
    public abstract class BaseException : Exception
    {
        public string ErrorCode { get; }
        public object[] Parameters { get; }

        protected BaseException(string message, string errorCode, params object[] parameters)
            : base(message)
        {
            ErrorCode = errorCode;
            Parameters = parameters;
        }

        protected BaseException(string message, string errorCode, Exception innerException, params object[] parameters)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            Parameters = parameters;
        }
    }

    public class ValidationException : BaseException
    {
        public ValidationException(string message)
            : base(message, "VAL001") { }
    }

    public class DatabaseException : BaseException
    {
        public DatabaseException(string message, Exception innerException)
            : base(message, "DB001", innerException) { }
    }
}