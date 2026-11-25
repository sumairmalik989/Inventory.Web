namespace Inventory.Web.ViewModels;

public sealed class ProductForm
{
    public int Id { get; set; }

    public string Sku { get; set; } = "";

    public string Name { get; set; } = "";

    public string? Description { get; set; }

    public int ReorderLevel { get; set; }
}
