using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KomunalkaAPI.Migrations
{
    /// <inheritdoc />
    public partial class RefactorTariffToUseServiceProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_tariffs_meters_meter_id",
                table: "tariffs");

            migrationBuilder.RenameColumn(
                name: "meter_id",
                table: "tariffs",
                newName: "utility_type_id");

            migrationBuilder.RenameIndex(
                name: "ix_tariffs_meter_id",
                table: "tariffs",
                newName: "ix_tariffs_utility_type_id");

            migrationBuilder.AddColumn<int>(
                name: "service_provider_id",
                table: "tariffs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_tariffs_service_provider_id",
                table: "tariffs",
                column: "service_provider_id");

            migrationBuilder.AddForeignKey(
                name: "fk_tariffs_service_providers_service_provider_id",
                table: "tariffs",
                column: "service_provider_id",
                principalTable: "service_providers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_tariffs_utility_types_utility_type_id",
                table: "tariffs",
                column: "utility_type_id",
                principalTable: "utility_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_tariffs_service_providers_service_provider_id",
                table: "tariffs");

            migrationBuilder.DropForeignKey(
                name: "fk_tariffs_utility_types_utility_type_id",
                table: "tariffs");

            migrationBuilder.DropIndex(
                name: "ix_tariffs_service_provider_id",
                table: "tariffs");

            migrationBuilder.DropColumn(
                name: "service_provider_id",
                table: "tariffs");

            migrationBuilder.RenameColumn(
                name: "utility_type_id",
                table: "tariffs",
                newName: "meter_id");

            migrationBuilder.RenameIndex(
                name: "ix_tariffs_utility_type_id",
                table: "tariffs",
                newName: "ix_tariffs_meter_id");

            migrationBuilder.AddForeignKey(
                name: "fk_tariffs_meters_meter_id",
                table: "tariffs",
                column: "meter_id",
                principalTable: "meters",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
