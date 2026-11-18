using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Slug = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ShortText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Body = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: false),
                    Icon = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CoverImage = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceMedia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServiceId = table.Column<int>(type: "integer", nullable: false),
                    Kind = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    YouTubeId = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceMedia_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "SiteTexts",
                columns: new[] { "Id", "Description", "Value" },
                values: new object[,]
                {
                    { "notFound.back", "Side der ikke findes – knap til forsiden", "Til forsiden" },
                    { "notFound.text", "Side der ikke findes – tekst", "Siden er måske flyttet eller slettet. Se alle ydelser på forsiden." },
                    { "notFound.title", "Side der ikke findes – overskrift", "Siden findes ikke" },
                    { "service.back", "Ydelsesside – link tilbage til alle ydelser", "Alle ydelser" },
                    { "service.ctaButton", "Ydelsesside – knap der åbner kontaktboksen", "Skriv til Carl" },
                    { "service.ctaText", "Ydelsesside – tekst i kontaktboksen", "Ring eller skriv og fortæl, hvad du har brug for." },
                    { "service.ctaTitle", "Ydelsesside – overskrift i kontaktboksen", "Skal Carl hjælpe med opgaven?" },
                    { "service.mediaTitle", "Ydelsesside – overskrift over billeder og videoer", "Billeder og video" },
                    { "service.otherTitle", "Ydelsesside – overskrift over de andre ydelser", "Andre ydelser" },
                    { "services.readMore", "Link på ydelseskortene til ydelsens egen side", "Læs mere" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceMedia_ServiceId_SortOrder",
                table: "ServiceMedia",
                columns: new[] { "ServiceId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Services_Slug",
                table: "Services",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Services_SortOrder",
                table: "Services",
                column: "SortOrder");

            // Data: de 6 ydelser, der indtil nu laa som tekster (services.N.title / services.N.text),
            // flyttes over med deres nuvaerende tekst (som den staar i databasen) og deres nuvaerende ikon.
            // Kortets tekst bruges ogsaa som start-tekst paa ydelsens egen side. De gamle tekster roeres ikke.
            // Adressen laves som i Content/Slug.cs: smaa bogstaver, ae/oe/aa, alt andet -> bindestreg.
            migrationBuilder.Sql("""
                INSERT INTO "Services" ("Slug", "Title", "ShortText", "Body", "Icon", "CoverImage", "SortOrder", "CreatedAt", "UpdatedAt")
                SELECT
                    CASE WHEN d.rn = 1 THEN d.base ELSE d.base || '-' || d.rn END,
                    d.title, d.text, d.text, d.icon, NULL, d.n - 1, now(), now()
                FROM (
                    SELECT b.*, row_number() OVER (PARTITION BY b.base ORDER BY b.n) AS rn
                    FROM (
                        SELECT s.n, s.icon, s.title, s.text,
                            COALESCE(NULLIF(left(btrim(regexp_replace(
                                lower(replace(replace(replace(replace(replace(replace(s.title,
                                    'Æ', 'ae'), 'æ', 'ae'), 'Ø', 'oe'), 'ø', 'oe'), 'Å', 'aa'), 'å', 'aa')),
                                '[^a-z0-9]+', '-', 'g'), '-'), 70), ''), 'ydelse-' || s.n) AS base
                        FROM (
                            SELECT v.n, v.icon,
                                left(COALESCE(NULLIF(btrim(t."Value"), ''), v.fallback), 120) AS title,
                                left(COALESCE(btrim(x."Value"), ''), 2000) AS text
                            FROM (VALUES
                                (1, 'roof', 'Tagarbejde'),
                                (2, 'deck', 'Terrasser'),
                                (3, 'carport', 'Tilbygninger og carporte'),
                                (4, 'window', 'Vinduer og døre'),
                                (5, 'interior', 'Indvendigt tømrerarbejde'),
                                (6, 'facade', 'Facader og beklædning')
                            ) AS v(n, icon, fallback)
                            LEFT JOIN "SiteTexts" t ON t."Id" = 'services.' || v.n || '.title'
                            LEFT JOIN "SiteTexts" x ON x."Id" = 'services.' || v.n || '.text'
                        ) s
                    ) b
                ) d
                ORDER BY d.n
                ON CONFLICT ("Slug") DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceMedia");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "notFound.back");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "notFound.text");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "notFound.title");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "service.back");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "service.ctaButton");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "service.ctaText");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "service.ctaTitle");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "service.mediaTitle");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "service.otherTitle");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.readMore");
        }
    }
}
