using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPackageRelationToQrCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "package_id",
                table: "qr_codes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_qr_codes_package_id",
                table: "qr_codes",
                column: "package_id");

            migrationBuilder.AddForeignKey(
                name: "FK_qr_codes_packages_package_id",
                table: "qr_codes",
                column: "package_id",
                principalTable: "packages",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_qr_codes_packages_package_id",
                table: "qr_codes");

            migrationBuilder.DropIndex(
                name: "IX_qr_codes_package_id",
                table: "qr_codes");

            migrationBuilder.DropColumn(
                name: "package_id",
                table: "qr_codes");
        }
    }
}
