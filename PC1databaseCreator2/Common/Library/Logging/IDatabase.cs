namespace PC1databaseCreator.Common.Infrastructure.Interfaces
{
    public interface IDatabase
    {
        Task<IResult<IEnumerable<T>>> QueryAsync<T>(string sql, object? param = null);
        Task<IResult<T>> QuerySingleAsync<T>(string sql, object? param = null);
        Task<IResult<int>> ExecuteAsync(string sql, object? param = null);
        Task<IResult> ExecuteTransactionAsync(Func<Task<IResult>> operation);

        IQueryable<T> Query<T>() where T : class;
        void Add<T>(T entity) where T : class;
        void Update<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        Task<IResult> SaveChangesAsync();
    }
}