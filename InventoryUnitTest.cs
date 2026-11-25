using Inventory.Application.Abstractions;
using Inventory.Application.Contracts;
using Inventory.Application.Services;
using Inventory.Domain.Entities;

namespace Tests;

public sealed class InventoryServiceTests
{
    [Fact]
    public async Task OutMovement_RejectsQuantityAboveAvailableStock()
    {
        var repository = new FakeInventoryRepository(available: 3);
        var service = new InventoryService(repository);

        var result = await service.RecordMovementAsync(1, MovementType.Out, 4, null);

        Assert.False(result.Success);
        Assert.Equal("Only 3 unit(s) are available.", result.Error);
        Assert.Equal(3, result.Available);
        Assert.Empty(repository.PendingMovements);
        Assert.Equal(0, repository.SaveAttempts);
    }

    [Fact]
    public async Task OutMovement_AllowsUsingExactlyTheAvailableStock()
    {
        var repository = new FakeInventoryRepository(available: 3);
        var service = new InventoryService(repository);

        var result = await service.RecordMovementAsync(1, MovementType.Out, 3, null);

        Assert.True(result.Success);
        Assert.Equal(0, result.Available);
        Assert.Equal(0, repository.CurrentStock);
        Assert.True(repository.Transaction.Committed);
    }

    [Fact]
    public async Task OutMovement_ConcurrentConflictRollsBackAndReturnsLatestStock()
    {
        var repository = new FakeInventoryRepository(available: 5)
        {
            ConflictOnSave = true,
            StockAfterConcurrentMovement = 1
        };
        var service = new InventoryService(repository);

        var result = await service.RecordMovementAsync(1, MovementType.Out, 4, null);

        Assert.False(result.Success);
        Assert.Equal(1, result.Available);
        Assert.Equal("Stock changed during this request. Current available stock is 1; please retry.", result.Error);
        Assert.True(repository.Transaction.RolledBack);
        Assert.Empty(repository.PendingMovements);
    }

    [Fact]
    public async Task Movement_RejectsNonPositiveQuantityBeforeOpeningTransaction()
    {
        var repository = new FakeInventoryRepository(available: 3);
        var service = new InventoryService(repository);

        var result = await service.RecordMovementAsync(1, MovementType.In, 0, null);

        Assert.False(result.Success);
        Assert.Equal("Quantity must be a positive integer.", result.Error);
        Assert.Equal(0, repository.TransactionBeginCount);
    }

    private sealed class FakeInventoryRepository(int available) : IInventoryRepository
    {
        private readonly Product _product = new() { Id = 1, Sku = "TEST-1", Name = "Test" };

        public int CurrentStock { get; private set; } = available;
        public bool ConflictOnSave { get; init; }
        public int StockAfterConcurrentMovement { get; init; }
        public int SaveAttempts { get; private set; }
        public int TransactionBeginCount { get; private set; }
        public List<StockMovement> PendingMovements { get; } = [];
        public FakeTransaction Transaction { get; } = new();

        public Task<List<ProductStock>> GetProductsAsync(string? search) => Task.FromResult(new List<ProductStock>());
        public Task<ProductStock?> GetProductAsync(int id) => Task.FromResult<ProductStock?>(null);
        public Task<bool> SkuExistsAsync(string sku, int? excludingId = null) => Task.FromResult(false);
        public Task<Product?> FindProductAsync(int id) => Task.FromResult<Product?>(_product);
        public Task<Product?> FindActiveProductForUpdateAsync(int id) => Task.FromResult<Product?>(_product);
        public Task<List<StockMovement>> GetMovementHistoryAsync(int productId) => Task.FromResult(new List<StockMovement>());
        public Task<int> GetCurrentStockAsync(int productId) => Task.FromResult(CurrentStock);
        public Task SaveProductAsync(Product product) => Task.CompletedTask;
        public Task<bool> DeactivateAsync(int id) => Task.FromResult(false);

        public Task<IInventoryTransaction> BeginTransactionAsync()
        {
            TransactionBeginCount++;
            return Task.FromResult<IInventoryTransaction>(Transaction);
        }

        public void AddStockMovement(StockMovement movement) => PendingMovements.Add(movement);

        public Task<bool> TrySaveChangesAsync()
        {
            SaveAttempts++;
            if (ConflictOnSave)
            {
                CurrentStock = StockAfterConcurrentMovement;
                return Task.FromResult(false);
            }

            foreach (var movement in PendingMovements)
                CurrentStock += movement.Type == MovementType.In ? movement.Quantity : -movement.Quantity;

            return Task.FromResult(true);
        }

        public void ClearTracking() => PendingMovements.Clear();

        public sealed class FakeTransaction : IInventoryTransaction
        {
            public bool Committed { get; private set; }
            public bool RolledBack { get; private set; }
            public Task CommitAsync() { Committed = true; return Task.CompletedTask; }
            public Task RollbackAsync() { RolledBack = true; return Task.CompletedTask; }
            public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        }
    }
}
