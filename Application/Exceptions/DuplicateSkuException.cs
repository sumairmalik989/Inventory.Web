namespace Inventory.Application.Exceptions;

public sealed class DuplicateSkuException(Exception innerException)
    : Exception("A product with this SKU already exists.", innerException);