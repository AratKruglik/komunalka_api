using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KomunalkaAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUtilityTypesAndMetersFromSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tariffs_Currencies_CurrencyId",
                table: "Tariffs");

            migrationBuilder.DropForeignKey(
                name: "FK_Tariffs_ServiceCategories_ServiceCategoryId",
                table: "Tariffs");

            migrationBuilder.DropForeignKey(
                name: "FK_Tariffs_ServiceCounterMeasurements_ServiceCounterMeasuremen~",
                table: "Tariffs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tariffs",
                table: "Tariffs");

            migrationBuilder.DropIndex(
                name: "IX_Tariffs_ServiceCategoryId",
                table: "Tariffs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Currencies",
                table: "Currencies");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "Tariffs");

            migrationBuilder.DropColumn(
                name: "ServiceCategoryId",
                table: "Tariffs");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "Tariffs");

            migrationBuilder.RenameTable(
                name: "Tariffs",
                newName: "tariffs");

            migrationBuilder.RenameTable(
                name: "Currencies",
                newName: "currencies");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "tariffs",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "tariffs",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CurrencyId",
                table: "tariffs",
                newName: "currency_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "tariffs",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ServiceCounterMeasurementId",
                table: "tariffs",
                newName: "meter_id");

            migrationBuilder.RenameIndex(
                name: "IX_Tariffs_ServiceCounterMeasurementId",
                table: "tariffs",
                newName: "IX_tariffs_meter_id");

            migrationBuilder.RenameIndex(
                name: "IX_Tariffs_CurrencyId",
                table: "tariffs",
                newName: "IX_tariffs_currency_id");

            migrationBuilder.RenameColumn(
                name: "Symbol",
                table: "currencies",
                newName: "symbol");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "currencies",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "currencies",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "currencies",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "currencies",
                newName: "created_at");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "tariffs",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "tariffs",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<decimal>(
                name: "base_rate",
                table: "tariffs",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "effective_from",
                table: "tariffs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "effective_to",
                table: "tariffs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "tariffs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pricing_model",
                table: "tariffs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "service_fee",
                table: "tariffs",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "symbol",
                table: "currencies",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "currencies",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "currencies",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "currencies",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "currencies",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tariffs",
                table: "tariffs",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_currencies",
                table: "currencies",
                column: "id");

            migrationBuilder.CreateTable(
                name: "utility_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    slug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    display_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_utility_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "meters",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    address_id = table.Column<int>(type: "integer", nullable: false),
                    utility_type_id = table.Column<int>(type: "integer", nullable: false),
                    serial_number = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meters", x => x.id);
                    table.ForeignKey(
                        name: "FK_meters_Addresses_address_id",
                        column: x => x.address_id,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_meters_utility_types_utility_type_id",
                        column: x => x.utility_type_id,
                        principalTable: "utility_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_meters_address_id",
                table: "meters",
                column: "address_id");

            migrationBuilder.CreateIndex(
                name: "IX_meters_utility_type_id",
                table: "meters",
                column: "utility_type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_tariffs_currencies_currency_id",
                table: "tariffs",
                column: "currency_id",
                principalTable: "currencies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tariffs_meters_meter_id",
                table: "tariffs",
                column: "meter_id",
                principalTable: "meters",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tariffs_currencies_currency_id",
                table: "tariffs");

            migrationBuilder.DropForeignKey(
                name: "FK_tariffs_meters_meter_id",
                table: "tariffs");

            migrationBuilder.DropTable(
                name: "meters");

            migrationBuilder.DropTable(
                name: "utility_types");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tariffs",
                table: "tariffs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_currencies",
                table: "currencies");

            migrationBuilder.DropColumn(
                name: "base_rate",
                table: "tariffs");

            migrationBuilder.DropColumn(
                name: "effective_from",
                table: "tariffs");

            migrationBuilder.DropColumn(
                name: "effective_to",
                table: "tariffs");

            migrationBuilder.DropColumn(
                name: "notes",
                table: "tariffs");

            migrationBuilder.DropColumn(
                name: "pricing_model",
                table: "tariffs");

            migrationBuilder.DropColumn(
                name: "service_fee",
                table: "tariffs");

            migrationBuilder.DropColumn(
                name: "code",
                table: "currencies");

            migrationBuilder.RenameTable(
                name: "tariffs",
                newName: "Tariffs");

            migrationBuilder.RenameTable(
                name: "currencies",
                newName: "Currencies");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Tariffs",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Tariffs",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "currency_id",
                table: "Tariffs",
                newName: "CurrencyId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Tariffs",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "meter_id",
                table: "Tariffs",
                newName: "ServiceCounterMeasurementId");

            migrationBuilder.RenameIndex(
                name: "IX_tariffs_meter_id",
                table: "Tariffs",
                newName: "IX_Tariffs_ServiceCounterMeasurementId");

            migrationBuilder.RenameIndex(
                name: "IX_tariffs_currency_id",
                table: "Tariffs",
                newName: "IX_Tariffs_CurrencyId");

            migrationBuilder.RenameColumn(
                name: "symbol",
                table: "Currencies",
                newName: "Symbol");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Currencies",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Currencies",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Currencies",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Currencies",
                newName: "CreatedAt");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Tariffs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Tariffs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AddressId",
                table: "Tariffs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ServiceCategoryId",
                table: "Tariffs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Value",
                table: "Tariffs",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Symbol",
                table: "Currencies",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Currencies",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Currencies",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Currencies",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tariffs",
                table: "Tariffs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Currencies",
                table: "Currencies",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Tariffs_ServiceCategoryId",
                table: "Tariffs",
                column: "ServiceCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tariffs_Currencies_CurrencyId",
                table: "Tariffs",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tariffs_ServiceCategories_ServiceCategoryId",
                table: "Tariffs",
                column: "ServiceCategoryId",
                principalTable: "ServiceCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tariffs_ServiceCounterMeasurements_ServiceCounterMeasuremen~",
                table: "Tariffs",
                column: "ServiceCounterMeasurementId",
                principalTable: "ServiceCounterMeasurements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
