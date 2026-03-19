using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class LinkQrExportsToFinancialTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "financial_transaction_id",
                table: "qr_codes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "created_by_admin_user_id",
                table: "financial_transactions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "created_by_library_account_id",
                table: "financial_transactions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_qr_codes_financial_transaction_id",
                table: "qr_codes",
                column: "financial_transaction_id");

            migrationBuilder.CreateIndex(
                name: "IX_financial_transactions_created_by_library_account_id",
                table: "financial_transactions",
                column: "created_by_library_account_id");

            migrationBuilder.AddForeignKey(
                name: "FK_financial_transactions_library_accounts_created_by_library_~",
                table: "financial_transactions",
                column: "created_by_library_account_id",
                principalTable: "library_accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_qr_codes_financial_transactions_financial_transaction_id",
                table: "qr_codes",
                column: "financial_transaction_id",
                principalTable: "financial_transactions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_financial_transactions_library_accounts_created_by_library_~",
                table: "financial_transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_qr_codes_financial_transactions_financial_transaction_id",
                table: "qr_codes");

            migrationBuilder.DropIndex(
                name: "IX_qr_codes_financial_transaction_id",
                table: "qr_codes");

            migrationBuilder.DropIndex(
                name: "IX_financial_transactions_created_by_library_account_id",
                table: "financial_transactions");

            migrationBuilder.DropColumn(
                name: "financial_transaction_id",
                table: "qr_codes");

            migrationBuilder.DropColumn(
                name: "created_by_library_account_id",
                table: "financial_transactions");

            migrationBuilder.AlterColumn<int>(
                name: "created_by_admin_user_id",
                table: "financial_transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
