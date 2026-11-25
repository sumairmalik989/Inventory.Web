using Inventory.Application.Contracts;
using Inventory.Domain.Entities;

namespace Inventory.Application.Abstractions;

public interface IInventoryService
{
    Task<List<ProductStock>> GetProductsAsync(string? search);
    Task<ProductStock?> GetProductAsync(int id);
    Task<bool> SkuExistsAsync(string sku, int? excludingId = null);
    Task<Product?> FindProductAsync(int id);
    Task<List<StockMovement>> GetMovementHistoryAsync(int productId);
    Task SaveProductAsync(Product product);
    Task<MovementResult> RecordMovementAsync(int productId, MovementType type, int quantity, string? note);
    Task<bool> DeactivateAsync(int id);
}
