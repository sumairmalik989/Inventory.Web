using Inventory.Domain.Entities;

namespace Inventory.Web.Data;

public static class SeedData
{
    public static async Task InitializeAsync(InventoryDbContext db)
    {
        if (db.Products.Any()) return;
        var keyboard = new Product { Sku = "KEY-001", Name = "Keyboard", ReorderLevel = 5 };
        var mouse = new Product { Sku = "MOU-001", Name = "Mouse", ReorderLevel = 10 };
        db.Products.AddRange(keyboard, mouse);
        await db.SaveChangesAsync();
        db.StockMovements.AddRange(
            new StockMovement { ProductId = keyboard.Id, Type = MovementType.In, Quantity = 12, Note = "Opening stock" },
            new StockMovement { ProductId = mouse.Id, Type = MovementType.In, Quantity = 8, Note = "Opening stock" });
        await db.SaveChangesAsync();
    }
}
