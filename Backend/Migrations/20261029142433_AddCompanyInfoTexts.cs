using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyInfoTexts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "SiteTexts",
                columns: new[] { "Id", "Description", "Value" },
                values: new object[,]
                {
                    { "company.address", "Adresse i bunden af siden (gade, postnr. by)", "Sviegade 2B, 1., 6760 Ribe" },
                    { "company.cvr", "CVR-nummer i bunden af siden", "43633104" },
                    { "company.legalName", "Juridisk navn/virksomhedsform i bunden af siden", "Carlton på Taget, enkeltmandsvirksomhed v/Carl Anton Emmertsen Lund" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "company.address");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "company.cvr");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "company.legalName");
        }
    }
}
