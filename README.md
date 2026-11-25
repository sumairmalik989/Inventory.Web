# Inventory

## Requirements

- .NET 10 SDK
- SQL Server; configure the `Inventory` connection string in `appsettings.json`

Apply pending migrations from Visual Studio Package Manager Console:

```powershell
Update-Database -Project Inventory.Web -StartupProject Inventory.Web
```

Run the web project with `dotnet run` from the `Inventory.Web` project directory.

## Design notes

Stock is derived from the movement ledger: inbound quantities minus outbound quantities. An outbound movement is rejected when it exceeds available stock. Each movement updates the product's `StockVersion` concurrency token in the same transaction as the movement insert. If a concurrent request wins, the losing transaction is rolled back and the latest available stock is reported.

Products are soft-deactivated (`IsActive = false`) rather than hard-deleted. This keeps stock-movement history and its product relationship intact, preserves auditability, and records the deactivation time in `DeletedDate`.

SKU uniqueness is checked before saving for a friendly validation message and enforced by a database unique index to protect against concurrent duplicate submissions.

## Tests

The xUnit project tests stock aggregation with in-memory SQLite and service behavior with a fake repository, including overdraw rejection and simulated optimistic-concurrency conflicts. From the `Inventory.Web` project directory, run:

```powershell
dotnet test ..\Tests\Tests.csproj
```

## AI notes

- **Tools used:** Chatgpt used to generate the initial project structure.
