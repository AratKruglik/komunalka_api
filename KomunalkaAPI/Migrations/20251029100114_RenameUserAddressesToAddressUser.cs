using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KomunalkaAPI.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserAddressesToAddressUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_addresses_addresses_address_id",
                table: "user_addresses");

            migrationBuilder.DropForeignKey(
                name: "fk_user_addresses_users_user_id",
                table: "user_addresses");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_addresses",
                table: "user_addresses");

            migrationBuilder.RenameTable(
                name: "user_addresses",
                newName: "address_user");

            migrationBuilder.RenameIndex(
                name: "IX_user_addresses_user_id_address_id",
                table: "address_user",
                newName: "IX_address_user_user_id_address_id");

            migrationBuilder.RenameIndex(
                name: "ix_user_addresses_address_id",
                table: "address_user",
                newName: "ix_address_user_address_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_address_user",
                table: "address_user",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_address_user_addresses_address_id",
                table: "address_user",
                column: "address_id",
                principalTable: "addresses",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_address_user_users_user_id",
                table: "address_user",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_address_user_addresses_address_id",
                table: "address_user");

            migrationBuilder.DropForeignKey(
                name: "fk_address_user_users_user_id",
                table: "address_user");

            migrationBuilder.DropPrimaryKey(
                name: "pk_address_user",
                table: "address_user");

            migrationBuilder.RenameTable(
                name: "address_user",
                newName: "user_addresses");

            migrationBuilder.RenameIndex(
                name: "IX_address_user_user_id_address_id",
                table: "user_addresses",
                newName: "IX_user_addresses_user_id_address_id");

            migrationBuilder.RenameIndex(
                name: "ix_address_user_address_id",
                table: "user_addresses",
                newName: "ix_user_addresses_address_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_addresses",
                table: "user_addresses",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_addresses_addresses_address_id",
                table: "user_addresses",
                column: "address_id",
                principalTable: "addresses",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_addresses_users_user_id",
                table: "user_addresses",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
