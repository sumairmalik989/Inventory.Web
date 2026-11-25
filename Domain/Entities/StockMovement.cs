using System.ComponentModel.DataAnnotations;

namespace Inventory.Domain.Entities;

public enum MovementType { In = 1, Out = 2 }

public sealed class StockMovement
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public MovementType Type { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
    public DateTime? DeletedDate { get; set; }
}
