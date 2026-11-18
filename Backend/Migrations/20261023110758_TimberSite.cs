using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class TimberSite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Compatibilities");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "OptionValues");

            migrationBuilder.DropTable(
                name: "Options");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.CreateTable(
                name: "SiteTexts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteTexts", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SiteTexts",
                columns: new[] { "Id", "Description", "Value" },
                values: new object[,]
                {
                    { "about.body", "Brødtekst i Om mig (tom linje = nyt afsnit)", "Skriv lidt om dig selv og firmaet her.\n\nTom linje giver nyt afsnit." },
                    { "about.title", "Overskrift i Om mig", "Om mig" },
                    { "company.name", "Firmanavn i navbar og footer", "Byg & Hyg" },
                    { "contact.email", "E-mail i kontakt-boksen", "bygoghyg.is@gmail.com" },
                    { "contact.facebookLabel", "Tekst på Facebook-linket", "Facebook" },
                    { "contact.facebookUrl", "Adresse på Facebook-siden", "https://www.facebook.com/" },
                    { "contact.phone", "Telefonnummer i kontakt-boksen", "+45 50 36 63 92" },
                    { "contact.title", "Overskrift i kontakt-boksen", "Kontakt os i dag" },
                    { "footer.text", "Tekst i bunden af siden", "Alle rettigheder forbeholdes" },
                    { "gallery.empty", "Vises når galleriet er tomt", "Der er ingen billeder endnu." },
                    { "gallery.subtitle", "Undertekst i Billeder", "Et udvalg af opgaver jeg har lavet" },
                    { "gallery.title", "Overskrift i Billeder", "Billeder" },
                    { "hero.image", "Baggrundsbillede på forsiden, fx /gallery/forside.jpg (tom = første billede i galleriet)", "" },
                    { "hero.subtitle", "Undertekst på forsiden", "Tømrerarbejde, terrasser og byggeri – udført med omhu" },
                    { "hero.title", "Stor overskrift på forsiden", "Håndværk i træ" },
                    { "nav.about", "Menupunkt til Om mig", "Om mig" },
                    { "nav.contact", "Kontakt-knap i navbar", "Kontakt" },
                    { "nav.gallery", "Menupunkt til Billeder", "Billeder" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteTexts");

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductCategory = table.Column<string>(type: "text", nullable: true),
                    ProductDescription = table.Column<string>(type: "text", nullable: true),
                    ProductDetailTitle = table.Column<string>(type: "text", nullable: true),
                    ProductImageUrl = table.Column<string>(type: "text", nullable: true),
                    Slug = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<string>(type: "text", nullable: false),
                    ProductCategory = table.Column<string>(type: "text", nullable: true),
                    ProductDescription = table.Column<string>(type: "text", nullable: true),
                    ProductDescriptionForAi = table.Column<string>(type: "text", nullable: true),
                    ProductDetailTitle = table.Column<string>(type: "text", nullable: true),
                    ProductDiscount = table.Column<int>(type: "integer", nullable: true),
                    ProductImageUrl = table.Column<string>(type: "text", nullable: true),
                    ProductName = table.Column<string>(type: "text", nullable: true),
                    ProductPrice = table.Column<int>(type: "integer", nullable: false),
                    ProductStock = table.Column<int>(type: "integer", nullable: true),
                    CustomProperties_Color = table.Column<string>(type: "text", nullable: true),
                    CustomProperties_Material = table.Column<string>(type: "text", nullable: true),
                    CustomProperties_Shape = table.Column<string>(type: "text", nullable: true),
                    CustomProperties_Size = table.Column<string>(type: "text", nullable: true),
                    CustomProperties_Style = table.Column<string>(type: "text", nullable: true),
                    CustomProperties_Theme = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                });

            migrationBuilder.CreateTable(
                name: "Options",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryModelId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Options", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Options_Categories_CategoryModelId",
                        column: x => x.CategoryModelId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OptionValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OptionId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptionValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptionValues_Options_OptionId",
                        column: x => x.OptionId,
                        principalTable: "Options",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Compatibilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OptionValueIdA = table.Column<int>(type: "integer", nullable: false),
                    OptionValueIdB = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compatibilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Compatibilities_OptionValues_OptionValueIdA",
                        column: x => x.OptionValueIdA,
                        principalTable: "OptionValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Compatibilities_OptionValues_OptionValueIdB",
                        column: x => x.OptionValueIdB,
                        principalTable: "OptionValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Compatibilities_OptionValueIdA",
                table: "Compatibilities",
                column: "OptionValueIdA");

            migrationBuilder.CreateIndex(
                name: "IX_Compatibilities_OptionValueIdB",
                table: "Compatibilities",
                column: "OptionValueIdB");

            migrationBuilder.CreateIndex(
                name: "IX_Options_CategoryModelId",
                table: "Options",
                column: "CategoryModelId");

            migrationBuilder.CreateIndex(
                name: "IX_OptionValues_OptionId",
                table: "OptionValues",
                column: "OptionId");
        }
    }
}
