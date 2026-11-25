using Inventory.Application.Abstractions;
using Inventory.Application.Contracts;
using Inventory.Application.Exceptions;
using Inventory.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Inventory.Web.Data;

public sealed class InventoryRepository(InventoryDbContext db) : IInventoryRepository
{
    public async Task<List<ProductStock>> GetProductsAsync(string? search)
    {
        var products = db.Products.AsNoTracking().Where(product => product.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            products = products.Where(product =>
                product.Sku.Contains(term) || product.Name.Contains(term));
        }

        return await products
            .OrderBy(product => product.Name)
            .Select(product => new ProductStock(
                product,
                db.StockMovements
                    .Where(movement => movement.ProductId == product.Id)
                    .Sum(movement => movement.Type == MovementType.In
                        ? movement.Quantity
                        : -movement.Quantity)))
            .ToListAsync();
    }

    public Task<ProductStock?> GetProductAsync(int id) =>
        db.Products
            .AsNoTracking()
            .Where(product => product.Id == id && product.IsActive)
            .Select(product => new ProductStock(
                product,
                db.StockMovements
                    .Where(movement => movement.ProductId == product.Id)
                    .Sum(movement => movement.Type == MovementType.In
                        ? movement.Quantity
                        : -movement.Quantity)))
            .FirstOrDefaultAsync();

    public Task<bool> SkuExistsAsync(string sku, int? excludingId = null) =>
        db.Products.AnyAsync(product =>
            product.Sku == sku && (!excludingId.HasValue || product.Id != excludingId.Value));

    public Task<Product?> FindProductAsync(int id) =>
        db.Products.FirstOrDefaultAsync(product => product.Id == id && product.IsActive);

    public Task<Product?> FindActiveProductForUpdateAsync(int id) =>
        db.Products.FirstOrDefaultAsync(product => product.Id == id && product.IsActive);

    public Task<List<StockMovement>> GetMovementHistoryAsync(int productId) =>
        db.StockMovements
            .AsNoTracking()
            .Where(movement => movement.ProductId == productId)
            .OrderByDescending(movement => movement.CreatedUtc)
            .ThenByDescending(movement => movement.Id)
            .ToListAsync();

    public Task<int> GetCurrentStockAsync(int productId) =>
        db.StockMovements
            .Where(movement => movement.ProductId == productId)
            .SumAsync(movement => movement.Type == MovementType.In
                ? movement.Quantity
                : -movement.Quantity);

    public async Task SaveProductAsync(Product product)
    {
        if (product.Id == 0)
            await db.Products.AddAsync(product);
        else
        {
            product.UpdatedDate = DateTime.UtcNow;
            db.Products.Update(product);
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            throw new DuplicateSkuException(exception);
        }
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var product = await db.Products.FirstOrDefaultAsync(item => item.Id == id && item.IsActive);
        if (product is null) return false;

        var now = DateTime.UtcNow;
        product.IsActive = false;
        product.UpdatedDate = now;
        product.DeletedDate = now;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<IInventoryTransaction> BeginTransactionAsync() =>
        new InventoryTransaction(await db.Database.BeginTransactionAsync());

    public void AddStockMovement(StockMovement movement) => db.StockMovements.Add(movement);

    public async Task<bool> TrySaveChangesAsync()
    {
        try
        {
            await db.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }

    public void ClearTracking() => db.ChangeTracker.Clear();

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        for (Exception? cause = exception; cause is not null; cause = cause.InnerException)
        {
            if (cause is SqlException { Number: 2601 or 2627 }) return true;
        }

        return false;
    }

    private sealed class InventoryTransaction(IDbContextTransaction transaction) : IInventoryTransaction
    {
        public Task CommitAsync() => transaction.CommitAsync();

        public Task RollbackAsync() => transaction.RollbackAsync();

        public ValueTask DisposeAsync() => transaction.DisposeAsync();
    }
}