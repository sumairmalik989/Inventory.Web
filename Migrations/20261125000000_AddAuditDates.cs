using System;
using Inventory.Web.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Web.Migrations;

[DbContext(typeof(InventoryDbContext))]
[Migration("20261125000000_AddAuditDates")]
public partial class AddAuditDates : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "UpdatedDate",
            table: "Products",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "DeletedDate",
            table: "Products",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "UpdatedDate",
            table: "StockMovements",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "DeletedDate",
            table: "StockMovements",
            type: "datetime2",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "UpdatedDate", table: "Products");
        migrationBuilder.DropColumn(name: "DeletedDate", table: "Products");
        migrationBuilder.DropColumn(name: "UpdatedDate", table: "StockMovements");
        migrationBuilder.DropColumn(name: "DeletedDate", table: "StockMovements");
    }
}