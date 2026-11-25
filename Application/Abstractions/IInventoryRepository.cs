using Inventory.Application.Contracts;
using Inventory.Domain.Entities;

namespace Inventory.Application.Abstractions;

public interface IInventoryRepository
{
    Task<List<ProductStock>> GetProductsAsync(string? search);
    Task<ProductStock?> GetProductAsync(int id);
    Task<bool> SkuExistsAsync(string sku, int? excludingId = null);
    Task<Product?> FindProductAsync(int id);
    Task<Product?> FindActiveProductForUpdateAsync(int id);
    Task<List<StockMovement>> GetMovementHistoryAsync(int productId);
    Task<int> GetCurrentStockAsync(int productId);
    Task SaveProductAsync(Product product);
    Task<bool> DeactivateAsync(int id);
    Task<IInventoryTransaction> BeginTransactionAsync();
    void AddStockMovement(StockMovement movement);
    Task<bool> TrySaveChangesAsync();
    void ClearTracking();
}
