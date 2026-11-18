using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class CompatibleTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Compatibilities");
        }
    }
}
