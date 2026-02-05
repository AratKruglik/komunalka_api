using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KomunalkaAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAddressIdToServiceProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_meters_service_providers_service_provider_id",
                table: "meters");

            migrationBuilder.AddColumn<int>(
                name: "address_id",
                table: "service_providers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_service_providers_address_id",
                table: "service_providers",
                column: "address_id");

            migrationBuilder.AddForeignKey(
                name: "fk_meters_service_providers_service_provider_id",
                table: "meters",
                column: "service_provider_id",
                principalTable: "service_providers",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_service_providers_addresses_address_id",
                table: "service_providers",
                column: "address_id",
                principalTable: "addresses",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_meters_service_providers_service_provider_id",
                table: "meters");

            migrationBuilder.DropForeignKey(
                name: "fk_service_providers_addresses_address_id",
                table: "service_providers");

            migrationBuilder.DropIndex(
                name: "ix_service_providers_address_id",
                table: "service_providers");

            migrationBuilder.DropColumn(
                name: "address_id",
                table: "service_providers");

            migrationBuilder.AddForeignKey(
                name: "fk_meters_service_providers_service_provider_id",
                table: "meters",
                column: "service_provider_id",
                principalTable: "service_providers",
                principalColumn: "id");
        }
    }
}
