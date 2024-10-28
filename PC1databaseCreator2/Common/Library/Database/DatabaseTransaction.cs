using System.Data;
using PC1databaseCreator.Common.Infrastructure.Interfaces.Database;
using PC1databaseCreator.Common.Infrastructure.Interfaces.Logging;
using PC1databaseCreator.Common.Results;

namespace PC1databaseCreator.Common.Library.Database
{
    public sealed class DatabaseTransaction : IDatabaseTransaction
    {
        private readonly IDbTransaction _transaction;
        private readonly ILoggerService _logger;
        private bool _isCommitted;
        private bool _isDisposed;

        public DatabaseTransaction(IDbTransaction transaction, ILoggerService logger)
        {
            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        async Task<Result> IDatabaseTransaction.CommitAsync()
        {
            try
            {
                ThrowIfDisposed();
                ThrowIfCommitted();

                _transaction.Commit();
                _isCommitted = true;

                _logger.LogInformation("Transaction committed successfully");
                return Result.Success();
            }
            catch (Exception ex) when (ex is not ObjectDisposedException && ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "Failed to commit transaction");
                return Result.Failure(ex);
            }
        }

        async Task<Result> IDatabaseTransaction.RollbackAsync()
        {
            try
            {
                ThrowIfDisposed();
                ThrowIfCommitted();

                _transaction.Rollback();

                _logger.LogInformation("Transaction rolled back successfully");
                return Result.Success();
            }
            catch (Exception ex) when (ex is not ObjectDisposedException && ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "Failed to rollback transaction");
                return Result.Failure(ex);
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            if (!_isCommitted)
            {
                try
                {
                    _transaction.Rollback();
                    _logger.LogWarning("Uncommitted transaction rolled back during disposal");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to rollback transaction during disposal");
                }
            }

            _transaction.Dispose();
            _isDisposed = true;

            GC.SuppressFinalize(this);
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(DatabaseTransaction));
        }

        private void ThrowIfCommitted()
        {
            if (_isCommitted)
                throw new InvalidOperationException("Transaction is already committed");
        }
    }
}