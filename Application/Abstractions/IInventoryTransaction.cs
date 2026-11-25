namespace Inventory.Application.Abstractions;

public interface IInventoryTransaction : IAsyncDisposable
{
    Task CommitAsync();
    Task RollbackAsync();
}
