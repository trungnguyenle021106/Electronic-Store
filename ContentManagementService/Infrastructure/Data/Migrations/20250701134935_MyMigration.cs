using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContentManagementService.Migrations
{
    /// <inheritdoc />
    public partial class MyMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Filters",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Position = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Filters", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "FilterDetails",
                columns: table => new
                {
                    FilterID = table.Column<int>(type: "int", nullable: false),
                    ProductPropertyID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterDetails", x => new { x.FilterID, x.ProductPropertyID });
                    table.ForeignKey(
                        name: "FK_FilterDetails_Filters_FilterID",
                        column: x => x.FilterID,
                        principalTable: "Filters",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FilterDetails");

            migrationBuilder.DropTable(
                name: "Filters");
        }
    }
}
