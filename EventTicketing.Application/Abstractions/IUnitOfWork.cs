namespace EventTicketing.Application.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action,
        CancellationToken cancellationToken);

    Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken);
}
