namespace PC1databaseCreator.Common.Infrastructure.Interfaces.Core
{
    public interface IResult
    {
        bool IsSuccess { get; }
        IEnumerable<string> Messages { get; }
        Exception? Exception { get; }
    }

    public interface IResult<out T> : IResult
    {
        T? Value { get; }
    }
}