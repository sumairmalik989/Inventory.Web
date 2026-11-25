using Inventory.Application.Abstractions;
using Inventory.Application.Contracts;
using Inventory.Domain.Entities;

namespace Inventory.Application.Services;

public sealed class InventoryService(IInventoryRepository repository) : IInventoryService
{
    public Task<List<ProductStock>> GetProductsAsync(string? search) =>
        repository.GetProductsAsync(search);

    public Task<ProductStock?> GetProductAsync(int id) =>
        repository.GetProductAsync(id);

    public Task<bool> SkuExistsAsync(string sku, int? excludingId = null) =>
        repository.SkuExistsAsync(sku, excludingId);

    public Task<Product?> FindProductAsync(int id) =>
        repository.FindProductAsync(id);

    public Task<List<StockMovement>> GetMovementHistoryAsync(int productId) =>
        repository.GetMovementHistoryAsync(productId);

    public Task SaveProductAsync(Product product) =>
        repository.SaveProductAsync(product);

    public async Task<MovementResult> RecordMovementAsync(int productId, MovementType type, int quantity, string? note)
    {
        if (quantity <= 0) return new(false, "Quantity must be a positive integer.", 0);
        if (!Enum.IsDefined(type)) return new(false, "Select a valid movement type.", 0);

        await using var transaction = await repository.BeginTransactionAsync();
        var product = await repository.FindActiveProductForUpdateAsync(productId);
        if (product is null) return new(false, "Product was not found.", 0);

        var current = await repository.GetCurrentStockAsync(productId);
        if (type == MovementType.Out && quantity > current)
            return new(false, $"Only {current} unit(s) are available.", current);

        product.StockVersion++;
        product.UpdatedDate = DateTime.UtcNow;
        repository.AddStockMovement(new StockMovement
        {
            ProductId = productId,
            Type = type,
            Quantity = quantity,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            CreatedUtc = DateTime.UtcNow
        });

        try
        {
            if (!await repository.TrySaveChangesAsync())
            {
                await transaction.RollbackAsync();
                repository.ClearTracking();
                var latest = await repository.GetCurrentStockAsync(productId);
                return new(false, $"Stock changed during this request. Current available stock is {latest}; please retry.", latest);
            }

            await transaction.CommitAsync();
            return new(true, null, type == MovementType.In ? current + quantity : current - quantity);
        }
        catch
        {
            await transaction.RollbackAsync();
            repository.ClearTracking();
            throw;
        }
    }

    public Task<bool> DeactivateAsync(int id) =>
        repository.DeactivateAsync(id);
}
