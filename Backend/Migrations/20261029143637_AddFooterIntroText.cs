using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddFooterIntroText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "SiteTexts",
                columns: new[] { "Id", "Description", "Value" },
                values: new object[] { "company.footerText", "Kort tekst om firmaet i bunden af siden", "Carlton på Taget holder til i Ribe og arbejder i hele byen og omegnen. Carl laver tømrerarbejde, tag og terrasser – og du kan se færdige opgaver i galleriet her på siden, før du beslutter dig." });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "company.footerText");
        }
    }
}
