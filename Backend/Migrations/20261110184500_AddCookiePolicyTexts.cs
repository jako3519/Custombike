using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCookiePolicyTexts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "SiteTexts",
                columns: new[] { "Id", "Description", "Value" },
                values: new object[,]
                {
                    { "cookie.body", "Brødtekst på siden Cookiepolitik (tom linje = nyt afsnit, ## Overskrift ##)", "Carlton på Taget (CVR 43633104), Sviegade 2B, 1., 6760 Ribe, er dataansvarlig for behandlingen af personoplysninger på denne hjemmeside.\n\nVi bruger cookies og lignende teknikker. Nødvendige funktioner virker altid. Cookies til statistik og marketing (Google) sættes kun, hvis du giver samtykke.\n\n## Nødvendige ##\n\nDisse er nødvendige for, at siden virker, og kræver ikke samtykke.\n\nDit valg om cookies huskes i din browser (localStorage, navn carlton.consent) i 12 måneder, så vi ikke spørger dig igen ved hvert besøg.\n\nHvis du er logget ind som administrator, bruges en login-cookie (carlton.auth) til at holde dig logget ind.\n\n## Statistik og marketing (Google) ##\n\nKun med dit samtykke indlæser vi Google Tag Manager (beholder GTM-K2F2GWD6). Den kan starte tags fra Google Analytics (statistik) og Google Ads (marketing og annoncemåling).\n\nDe typiske cookies, Google kan sætte, er blandt andet:\n\nStatistik: _ga og _ga_* (typisk op til 13–24 måneder) samt _gid (24 timer). De bruges til at skelne besøg og forstå, hvordan siden bruges.\n\nMarketing: _gcl_au, _gcl_aw og lignende (typisk op til 90 dage). De bruges til at måle annoncer og eventuelt vise mere relevante annoncer.\n\nDen præcise liste afhænger af de tags, der er sat op i Google Tag Manager. Data sendes til Google Ireland Limited og kan blive videregivet til Google LLC i USA. Google behandler oplysningerne efter deres privatlivspolitik (policies.google.com/privacy).\n\n## Vores egen statistik ##\n\nSiden har også en indbygget, cookiefri besøgsstatistik, som ikke bruger cookies og ikke kræver samtykke. Den kører uafhængigt af Google.\n\n## Sådan trækker du samtykket tilbage ##\n\nDu kan altid ændre eller trække dit samtykke tilbage via linket \"Cookie-indstillinger\" i bunden af siden. Valget huskes i 12 måneder, eller indtil du ændrer det. Du kan også slette gemte data i din browser.\n\nHar du spørgsmål til vores brug af cookies, kan du kontakte os på de kontaktoplysninger, der står i bunden af siden." },
                    { "cookie.title", "Overskrift på siden Cookiepolitik", "Cookiepolitik" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "cookie.body");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "cookie.title");
        }
    }
}
