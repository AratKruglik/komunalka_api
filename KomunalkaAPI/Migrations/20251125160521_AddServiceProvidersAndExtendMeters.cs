using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KomunalkaAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceProvidersAndExtendMeters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "initial_reading",
                table: "meters",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "installation_date",
                table: "meters",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "location",
                table: "meters",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "model_name",
                table: "meters",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "meters",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "photo_path",
                table: "meters",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "service_provider_id",
                table: "meters",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "service_providers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_providers", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_meters_service_provider_id",
                table: "meters",
                column: "service_provider_id");

            migrationBuilder.AddForeignKey(
                name: "fk_meters_service_providers_service_provider_id",
                table: "meters",
                column: "service_provider_id",
                principalTable: "service_providers",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_meters_service_providers_service_provider_id",
                table: "meters");

            migrationBuilder.DropTable(
                name: "service_providers");

            migrationBuilder.DropIndex(
                name: "ix_meters_service_provider_id",
                table: "meters");

            migrationBuilder.DropColumn(
                name: "initial_reading",
                table: "meters");

            migrationBuilder.DropColumn(
                name: "installation_date",
                table: "meters");

            migrationBuilder.DropColumn(
                name: "location",
                table: "meters");

            migrationBuilder.DropColumn(
                name: "model_name",
                table: "meters");

            migrationBuilder.DropColumn(
                name: "notes",
                table: "meters");

            migrationBuilder.DropColumn(
                name: "photo_path",
                table: "meters");

            migrationBuilder.DropColumn(
                name: "service_provider_id",
                table: "meters");
        }
    }
}
