using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KomunalkaAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddOAuthSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "password",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<string>(
                name: "auth_provider",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "email_verified",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_login_at",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            // Update existing users to have AuthProvider = 'Local'
            migrationBuilder.Sql(
                @"UPDATE users
                  SET auth_provider = 'Local'
                  WHERE auth_provider IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_user_provider_external_id",
                table: "users",
                columns: new[] { "auth_provider", "external_id" },
                unique: true,
                filter: "auth_provider IS NOT NULL AND external_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_provider_external_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "auth_provider",
                table: "users");

            migrationBuilder.DropColumn(
                name: "email_verified",
                table: "users");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "last_login_at",
                table: "users");

            migrationBuilder.AlterColumn<string>(
                name: "password",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
