using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KomunalkaAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAvatar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "avatar_height",
                table: "users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "avatar_mime_type",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "avatar_optimized_path",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "avatar_size_in_bytes",
                table: "users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "avatar_thumbnail_path",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "avatar_width",
                table: "users",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "avatar_height",
                table: "users");

            migrationBuilder.DropColumn(
                name: "avatar_mime_type",
                table: "users");

            migrationBuilder.DropColumn(
                name: "avatar_optimized_path",
                table: "users");

            migrationBuilder.DropColumn(
                name: "avatar_size_in_bytes",
                table: "users");

            migrationBuilder.DropColumn(
                name: "avatar_thumbnail_path",
                table: "users");

            migrationBuilder.DropColumn(
                name: "avatar_width",
                table: "users");
        }
    }
}
