using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KomunalkaAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddMeterReadingImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "meter_reading_images",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    service_counter_value_id = table.Column<int>(type: "integer", nullable: false),
                    optimized_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    thumbnail_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    optimized_size_in_bytes = table.Column<long>(type: "bigint", nullable: false),
                    thumbnail_size_in_bytes = table.Column<long>(type: "bigint", nullable: false),
                    width = table.Column<int>(type: "integer", nullable: false),
                    height = table.Column<int>(type: "integer", nullable: false),
                    mime_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_processed = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_meter_reading_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_meter_reading_images_service_counter_values_service_counter_v~",
                        column: x => x.service_counter_value_id,
                        principalTable: "service_counter_values",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_meter_reading_images_service_counter_value_id",
                table: "meter_reading_images",
                column: "service_counter_value_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "meter_reading_images");
        }
    }
}
