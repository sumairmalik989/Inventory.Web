namespace Inventory.Application.Contracts;

public sealed record MovementResult(bool Success, string? Error, int Available);
