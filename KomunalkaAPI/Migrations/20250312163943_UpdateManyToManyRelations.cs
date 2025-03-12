using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KomunalkaAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateManyToManyRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServiceCategoryId",
                table: "Addresses",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AddressesServiceCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AddressId = table.Column<int>(type: "integer", nullable: false),
                    ServiceCategoryId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressesServiceCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AddressesServiceCategories_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AddressesServiceCategories_ServiceCategories_ServiceCategor~",
                        column: x => x.ServiceCategoryId,
                        principalTable: "ServiceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_ServiceCategoryId",
                table: "Addresses",
                column: "ServiceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressesServiceCategories_AddressId",
                table: "AddressesServiceCategories",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressesServiceCategories_ServiceCategoryId",
                table: "AddressesServiceCategories",
                column: "ServiceCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_ServiceCategories_ServiceCategoryId",
                table: "Addresses",
                column: "ServiceCategoryId",
                principalTable: "ServiceCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_ServiceCategories_ServiceCategoryId",
                table: "Addresses");

            migrationBuilder.DropTable(
                name: "AddressesServiceCategories");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_ServiceCategoryId",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "ServiceCategoryId",
                table: "Addresses");
        }
    }
}
