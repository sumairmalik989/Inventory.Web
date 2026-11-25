using Inventory.Domain.Entities;
using Inventory.Web.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Tests;

public sealed class InventoryRepositoryTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private InventoryDbContext _db = null!;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlite(_connection)
            .Options;
        _db = new InventoryDbContext(options);
        await _db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task GetCurrentStockAsync_SumsInboundAndOutboundMovements()
    {
        var product = new Product { Sku = "STOCK-1", Name = "Stock test" };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        _db.StockMovements.AddRange(
            new StockMovement { ProductId = product.Id, Type = MovementType.In, Quantity = 10 },
            new StockMovement { ProductId = product.Id, Type = MovementType.Out, Quantity = 4 },
            new StockMovement { ProductId = product.Id, Type = MovementType.In, Quantity = 2 });
        await _db.SaveChangesAsync();

        var repository = new InventoryRepository(_db);

        Assert.Equal(8, await repository.GetCurrentStockAsync(product.Id));
    }
}