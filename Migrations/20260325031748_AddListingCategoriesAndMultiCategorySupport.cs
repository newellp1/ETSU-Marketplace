using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETSU_Marketplace.Migrations
{
    /// <inheritdoc />
    public partial class AddListingCategoriesAndMultiCategorySupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ListingId",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "SenderDisplayName",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "SenderUserId",
                table: "ChatMessages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ListingId",
                table: "ChatMessages",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SenderDisplayName",
                table: "ChatMessages",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SenderUserId",
                table: "ChatMessages",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
