using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnalyticService.Migrations
{
    /// <inheritdoc />
    public partial class MyMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrdersByDates",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalOrders = table.Column<int>(type: "int", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CancelledOrders = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdersByDates", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ProductsStatistics",
                columns: table => new
                {
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    TotalSales = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductsStatistics", x => x.ProductID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrdersByDates_Date",
                table: "OrdersByDates",
                column: "Date",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrdersByDates");

            migrationBuilder.DropTable(
                name: "ProductsStatistics");
        }
    }
}
