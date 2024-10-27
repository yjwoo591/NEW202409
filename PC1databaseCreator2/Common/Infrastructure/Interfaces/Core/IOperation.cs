namespace PC1databaseCreator.Common.Infrastructure.Interfaces.Core
{
    public interface IOperation<in TInput, TOutput>
    {
        Task<IResult<TOutput>> ExecuteAsync(TInput input, CancellationToken cancellation = default);
        Task<IResult> ValidateAsync(TInput input, CancellationToken cancellation = default);
    }

    public interface IOperation<TOutput> : IOperation<Unit, TOutput>
    {
        new Task<IResult<TOutput>> ExecuteAsync(CancellationToken cancellation = default);
    }

    public record Unit
    {
        private Unit() { }
        public static Unit Value { get; } = new Unit();
    }
}