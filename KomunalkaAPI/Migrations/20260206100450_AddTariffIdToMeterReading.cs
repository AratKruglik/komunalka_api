using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KomunalkaAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTariffIdToMeterReading : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "tariff_id",
                table: "meter_readings",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_meter_readings_tariff_id",
                table: "meter_readings",
                column: "tariff_id");

            migrationBuilder.AddForeignKey(
                name: "fk_meter_readings_tariffs_tariff_id",
                table: "meter_readings",
                column: "tariff_id",
                principalTable: "tariffs",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_meter_readings_tariffs_tariff_id",
                table: "meter_readings");

            migrationBuilder.DropIndex(
                name: "ix_meter_readings_tariff_id",
                table: "meter_readings");

            migrationBuilder.DropColumn(
                name: "tariff_id",
                table: "meter_readings");
        }
    }
}
