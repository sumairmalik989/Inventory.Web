using Inventory.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Inventory.Web.ViewModels;

public sealed class MovementForm
{
    [Required]
    public MovementType? Type { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int? Quantity { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }
}
