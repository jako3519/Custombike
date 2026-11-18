using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddServicesTexts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "SiteTexts",
                columns: new[] { "Id", "Description", "Value" },
                values: new object[,]
                {
                    { "nav.services", "Menupunkt til Ydelser", "Ydelser" },
                    { "services.1.text", "Ydelse 1 – tekst", "Nyt tag, tagrenovering, udskiftning af tagplader og reparation af utætte tage." },
                    { "services.1.title", "Ydelse 1 – titel", "Tagarbejde" },
                    { "services.2.text", "Ydelse 2 – tekst", "Nye træterrasser, hævede terrasser og renovering af gamle terrasser." },
                    { "services.2.title", "Ydelse 2 – titel", "Terrasser" },
                    { "services.3.text", "Ydelse 3 – tekst", "Carporte, skure, overdækninger og mindre tilbygninger." },
                    { "services.3.title", "Ydelse 3 – titel", "Tilbygninger og carporte" },
                    { "services.4.text", "Ydelse 4 – tekst", "Udskiftning og montering af vinduer, døre og ovenlysvinduer." },
                    { "services.4.title", "Ydelse 4 – titel", "Vinduer og døre" },
                    { "services.5.text", "Ydelse 5 – tekst", "Gulve, lofter, skillevægge og efterisolering." },
                    { "services.5.title", "Ydelse 5 – titel", "Indvendigt tømrerarbejde" },
                    { "services.6.text", "Ydelse 6 – tekst", "Træbeklædning, sternbrædder og vindskeder." },
                    { "services.6.title", "Ydelse 6 – titel", "Facader og beklædning" },
                    { "services.note", "Linje under ydelserne om småopgaver", "Småopgaver og reparationer: Carl tager også mindre opgaver for private i Ribe og omegn." },
                    { "services.subtitle", "Undertekst i Ydelser", "Tømrerarbejde for private i Ribe og omegn – fra tag og terrasse til opgaver inden døre." },
                    { "services.title", "Overskrift i Ydelser", "Ydelser" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "nav.services");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.1.text");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.1.title");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.2.text");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.2.title");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.3.text");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.3.title");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.4.text");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.4.title");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.5.text");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.5.title");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.6.text");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.6.title");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.note");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.subtitle");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.title");
        }
    }
}
