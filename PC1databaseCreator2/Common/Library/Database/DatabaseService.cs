using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using PC1databaseCreator.Common.Infrastructure.Interfaces.Database;
using PC1databaseCreator.Common.Infrastructure.Interfaces.Logging;
using PC1databaseCreator.Common.Results;

namespace PC1databaseCreator.Common.Library.Database
{
    public sealed class DatabaseService : IDatabase
    {
        private readonly string _connectionString;
        private readonly ILoggerService _logger;

        public DatabaseService(string connectionString, ILoggerService logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        async Task<Result<IEnumerable<T>>> IDatabase.QueryAsync<T>(string sql, object? param)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = DatabaseCommand.CreateQuery(sql, param);
                var result = await connection.QueryAsync<T>(command.CommandText, command.Parameters);
                return Result<IEnumerable<T>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute query: {Sql}", sql);
                return Result<IEnumerable<T>>.Failure(ex);
            }
        }

        async Task<Result<T>> IDatabase.QuerySingleAsync<T>(string sql, object? param)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = DatabaseCommand.CreateQuery(sql, param);
                var result = await connection.QuerySingleOrDefaultAsync<T>(command.CommandText, command.Parameters);
                return result != null
                    ? Result<T>.Success(result)
                    : Result<T>.Failure("No data found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute single query: {Sql}", sql);
                return Result<T>.Failure(ex);
            }
        }

        async Task<Result<int>> IDatabase.ExecuteAsync(string sql, object? param)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = DatabaseCommand.CreateQuery(sql, param);
                var result = await connection.ExecuteAsync(command.CommandText, command.Parameters);
                return Result<int>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute command: {Sql}", sql);
                return Result<int>.Failure(ex);
            }
        }

        async Task<Result> IDatabase.ExecuteTransactionAsync(Func<Task<Result>> operation)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();
            using var dbTransaction = new DatabaseTransaction(transaction, _logger);

            try
            {
                var result = await operation();
                if (result.IsSuccess)
                {
                    var commitResult = await ((IDatabaseTransaction)dbTransaction).CommitAsync();
                    if (!commitResult.IsSuccess)
                    {
                        await ((IDatabaseTransaction)dbTransaction).RollbackAsync();
                        return Result.Failure("Failed to commit transaction");
                    }
                    return Result.Success();
                }

                await ((IDatabaseTransaction)dbTransaction).RollbackAsync();
                return result;
            }
            catch (Exception ex)
            {
                await ((IDatabaseTransaction)dbTransaction).RollbackAsync();
                _logger.LogError(ex, "Transaction failed");
                return Result.Failure(ex);
            }
        }

        async Task<Result<int>> IDatabase.ExecuteStoredProcedureAsync(string procName, object? param)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = DatabaseCommand.CreateStoredProcedure(procName, param);
                var result = await connection.ExecuteAsync(command.CommandText, command.Parameters,
                    commandType: command.Type);
                return Result<int>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute stored procedure: {ProcName}", procName);
                return Result<int>.Failure(ex);
            }
        }
    }
}