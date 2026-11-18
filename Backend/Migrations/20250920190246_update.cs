using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Options_Categories_CategoryId",
                table: "Options");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Options",
                newName: "CategoryModelId");

            migrationBuilder.RenameIndex(
                name: "IX_Options_CategoryId",
                table: "Options",
                newName: "IX_Options_CategoryModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Options_Categories_CategoryModelId",
                table: "Options",
                column: "CategoryModelId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Options_Categories_CategoryModelId",
                table: "Options");

            migrationBuilder.RenameColumn(
                name: "CategoryModelId",
                table: "Options",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Options_CategoryModelId",
                table: "Options",
                newName: "IX_Options_CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Options_Categories_CategoryId",
                table: "Options",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
