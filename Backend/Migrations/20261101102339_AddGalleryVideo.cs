using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddGalleryVideo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "GalleryImages",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<string>(
                name: "Kind",
                table: "GalleryImages",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "image");

            migrationBuilder.AddColumn<string>(
                name: "YouTubeId",
                table: "GalleryImages",
                type: "character varying(11)",
                maxLength: 11,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kind",
                table: "GalleryImages");

            migrationBuilder.DropColumn(
                name: "YouTubeId",
                table: "GalleryImages");

            // YouTube-rækker har intet filnavn og kan ikke beholdes uden video-understøttelse
            migrationBuilder.Sql("DELETE FROM \"GalleryImages\" WHERE \"FileName\" IS NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "GalleryImages",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);
        }
    }
}
