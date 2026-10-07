namespace PS.OrderEvents.Application.Abstractions
{

    public interface IUnitOfWork
    {
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
