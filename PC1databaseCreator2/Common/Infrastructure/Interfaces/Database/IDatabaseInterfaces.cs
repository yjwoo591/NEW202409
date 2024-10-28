using System.Data;
using PC1databaseCreator.Common.Results;

namespace PC1databaseCreator.Common.Infrastructure.Interfaces.Database
{
    public interface IDatabase
    {
        Task<Result<IEnumerable<T>>> QueryAsync<T>(string sql, object? param = null);
        Task<Result<T>> QuerySingleAsync<T>(string sql, object? param = null);
        Task<Result<int>> ExecuteAsync(string sql, object? param = null);
        Task<Result> ExecuteTransactionAsync(Func<Task<Result>> operation);
        Task<Result<int>> ExecuteStoredProcedureAsync(string procName, object? param = null);
    }

    public interface IDatabaseCommand
    {
        string CommandText { get; }
        object? Parameters { get; }
        CommandType Type { get; }
    }

    public interface IDatabaseTransaction : IDisposable
    {
        Task<Result> CommitAsync();
        Task<Result> RollbackAsync();
    }
}