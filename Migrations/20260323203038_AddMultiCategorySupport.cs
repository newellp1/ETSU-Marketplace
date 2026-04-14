using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETSU_Marketplace.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiCategorySupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Listings");

            migrationBuilder.CreateTable(
                name: "ListingCategories",
                columns: table => new
                {
                    ListingId = table.Column<int>(type: "INTEGER", nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListingCategories", x => new { x.ListingId, x.Category });
                    table.ForeignKey(
                        name: "FK_ListingCategories_Listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ListingCategories");

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Listings",
                type: "INTEGER",
                nullable: true);
        }
    }
}
