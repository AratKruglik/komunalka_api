using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KomunalkaAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUtilityTypeIdToServiceProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "utility_type_id",
                table: "service_providers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_service_providers_utility_type_id",
                table: "service_providers",
                column: "utility_type_id");

            migrationBuilder.AddForeignKey(
                name: "fk_service_providers_utility_types_utility_type_id",
                table: "service_providers",
                column: "utility_type_id",
                principalTable: "utility_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_service_providers_utility_types_utility_type_id",
                table: "service_providers");

            migrationBuilder.DropIndex(
                name: "ix_service_providers_utility_type_id",
                table: "service_providers");

            migrationBuilder.DropColumn(
                name: "utility_type_id",
                table: "service_providers");
        }
    }
}
