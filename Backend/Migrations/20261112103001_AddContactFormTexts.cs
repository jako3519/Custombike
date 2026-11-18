using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddContactFormTexts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "SiteTexts",
                columns: new[] { "Id", "Description", "Value" },
                values: new object[,]
                {
                    { "contact.formIntro", "Kort tekst over kontaktformularen", "Fortæl kort om opgaven, så ringer eller skriver Carl tilbage." },
                    { "contact.formTitle", "Overskrift over kontaktformularen", "Skriv til Carl" },
                    { "contact.gdpr", "Tekst under kontaktformularen om brug af oplysninger", "Dine oplysninger bruges kun til at besvare din henvendelse. Vi gemmer dem i op til 12 måneder." },
                    { "contact.submit", "Knap på kontaktformularen", "Send besked" },
                    { "contact.thanksText", "Tekst på tak-siden efter en henvendelse", "Carl har modtaget din besked og vender tilbage hurtigst muligt." },
                    { "contact.thanksTitle", "Overskrift på tak-siden efter en henvendelse", "Tak for din henvendelse" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "SiteTexts", keyColumn: "Id", keyValue: "contact.formIntro");
            migrationBuilder.DeleteData(table: "SiteTexts", keyColumn: "Id", keyValue: "contact.formTitle");
            migrationBuilder.DeleteData(table: "SiteTexts", keyColumn: "Id", keyValue: "contact.gdpr");
            migrationBuilder.DeleteData(table: "SiteTexts", keyColumn: "Id", keyValue: "contact.submit");
            migrationBuilder.DeleteData(table: "SiteTexts", keyColumn: "Id", keyValue: "contact.thanksText");
            migrationBuilder.DeleteData(table: "SiteTexts", keyColumn: "Id", keyValue: "contact.thanksTitle");
        }
    }
}
