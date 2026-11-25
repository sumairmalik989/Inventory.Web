using Inventory.Domain.Entities;

namespace Inventory.Application.Contracts;

public sealed record ProductStock(Product Product, int CurrentStock);
