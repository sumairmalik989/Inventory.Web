using FluentValidation;
using Inventory.Application.Abstractions;

namespace Inventory.Web.ViewModels;

public sealed class ProductFormValidator : AbstractValidator<ProductForm>
{
    public ProductFormValidator(IInventoryService inventory)
    {
        RuleFor(form => form.Sku)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("SKU is required.")
            .MaximumLength(40)
            .WithMessage("SKU cannot exceed 40 characters.")
            .MustAsync(async (form, sku, cancellationToken) =>
                !await inventory.SkuExistsAsync(sku.Trim(), form.Id == 0 ? null : form.Id))
            .WithMessage("A product with this SKU already exists.");

        RuleFor(form => form.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(120)
            .WithMessage("Name cannot exceed 120 characters.");

        RuleFor(form => form.Description)
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(form => form.ReorderLevel)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Reorder level cannot be negative.");
    }
}