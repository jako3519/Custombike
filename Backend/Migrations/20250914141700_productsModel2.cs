using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class productsModel2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomProperties_Color",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomProperties_Material",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomProperties_Shape",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomProperties_Size",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomProperties_Style",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomProperties_Theme",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductDetailTitle",
                table: "Products",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomProperties_Color",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CustomProperties_Material",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CustomProperties_Shape",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CustomProperties_Size",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CustomProperties_Style",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CustomProperties_Theme",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductDetailTitle",
                table: "Products");
        }
    }
}
