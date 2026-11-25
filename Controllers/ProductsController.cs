using Inventory.Application.Abstractions;
using Inventory.Application.Exceptions;
using Inventory.Domain.Entities;
using Inventory.Web.ViewModels;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.Controllers;

public sealed class ProductsController(
    IInventoryService inventory,
    IValidator<ProductForm> productFormValidator) : Controller
{
    public async Task<IActionResult> Index(string? search) =>
        View(await inventory.GetProductsAsync(search));

    [HttpGet]
    public IActionResult Create() => View(new ProductForm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductForm form)
    {
        form.Id = 0;
        await ValidateProductFormAsync(form);
        if (!ModelState.IsValid) return View(form);

        var sku = form.Sku.Trim();

        try
        {
            await inventory.SaveProductAsync(new Product
            {
                Sku = sku,
                Name = form.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim(),
                ReorderLevel = form.ReorderLevel
            });
        }
        catch (DuplicateSkuException)
        {
            ModelState.AddModelError(nameof(form.Sku), "A product with this SKU already exists.");
            return View(form);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await inventory.FindProductAsync(id);
        if (product is null) return NotFound();

        return View(new ProductForm
        {
            Id = product.Id,
            Sku = product.Sku,
            Name = product.Name,
            Description = product.Description,
            ReorderLevel = product.ReorderLevel
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductForm form)
    {
        if (id != form.Id) return BadRequest();

        var product = await inventory.FindProductAsync(id);
        if (product is null) return NotFound();

        await ValidateProductFormAsync(form);
        if (!ModelState.IsValid) return View(form);

        var sku = form.Sku.Trim();
        product.Sku = sku;
        product.Name = form.Name.Trim();
        product.Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim();
        product.ReorderLevel = form.ReorderLevel;
        try
        {
            await inventory.SaveProductAsync(product);
        }
        catch (DuplicateSkuException)
        {
            ModelState.AddModelError(nameof(form.Sku), "A product with this SKU already exists.");
            return View(form);
        }

        return RedirectToAction(nameof(Details), new { id = product.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await inventory.GetProductAsync(id);
        if (product is null) return NotFound();
        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!await inventory.DeactivateAsync(id)) return NotFound();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await inventory.GetProductAsync(id);
        if (product is null) return NotFound();
        ViewBag.Movements = await inventory.GetMovementHistoryAsync(id);
        ViewBag.MovementForm = new MovementForm();
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Move(int id, MovementForm form)
    {
        if (!ModelState.IsValid || !Enum.IsDefined(typeof(MovementType), form.Type.GetValueOrDefault()))
        {
            TempData["Error"] = "Enter a valid movement type, a positive quantity, and a note of 500 characters or fewer.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var result = await inventory.RecordMovementAsync(
            id,
            form.Type!.Value,
            form.Quantity!.Value,
            form.Note);

        if (!result.Success)
        {
            TempData["Error"] = result.Error ?? "The stock movement could not be recorded.";
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["Success"] = "Stock movement recorded successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task ValidateProductFormAsync(ProductForm form)
    {
        var result = await productFormValidator.ValidateAsync(form);
        foreach (var error in result.Errors)
            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
    }


}