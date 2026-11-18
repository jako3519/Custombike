using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaHashes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverHash",
                table: "Services",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentHash",
                table: "ServiceMedia",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentHash",
                table: "GalleryImages",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GalleryImages_ContentHash",
                table: "GalleryImages",
                column: "ContentHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GalleryImages_ContentHash",
                table: "GalleryImages");

            migrationBuilder.DropColumn(
                name: "CoverHash",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "ContentHash",
                table: "ServiceMedia");

            migrationBuilder.DropColumn(
                name: "ContentHash",
                table: "GalleryImages");
        }
    }
}
