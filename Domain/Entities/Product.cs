using System.ComponentModel.DataAnnotations;

namespace Inventory.Domain.Entities;

public sealed class Product
{
    public int Id { get; set; }

    [Required, StringLength(40)]
    public string Sku { get; set; } = "";

    [Required, StringLength(120)]
    public string Name { get; set; } = "";

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(0, int.MaxValue)]
    public int ReorderLevel { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
    public DateTime? DeletedDate { get; set; }

    // Incremented for every movement. Used as an optimistic concurrency token.
    public int StockVersion { get; set; }

    public List<StockMovement> Movements { get; set; } = [];
}
