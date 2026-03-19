using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class RefactorFinancialSettlements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_transaction_settlements_financial_transactions_payment_tran~",
                table: "transaction_settlements");

            migrationBuilder.DropForeignKey(
                name: "FK_transaction_settlements_financial_transactions_target_trans~",
                table: "transaction_settlements");

            migrationBuilder.DropIndex(
                name: "IX_transaction_settlements_payment_transaction_id",
                table: "transaction_settlements");

            migrationBuilder.DropColumn(
                name: "payment_transaction_id",
                table: "transaction_settlements");

            migrationBuilder.RenameColumn(
                name: "target_transaction_id",
                table: "transaction_settlements",
                newName: "financial_transaction_id");

            migrationBuilder.RenameIndex(
                name: "IX_transaction_settlements_target_transaction_id",
                table: "transaction_settlements",
                newName: "IX_transaction_settlements_financial_transaction_id");

            migrationBuilder.AddColumn<int>(
                name: "created_by_admin_user_id",
                table: "transaction_settlements",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "transaction_settlements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "quantity",
                table: "transaction_settlements",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "settlement_date",
                table: "transaction_settlements",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "settlement_mode",
                table: "transaction_settlements",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "unit_amount",
                table: "transaction_settlements",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_transaction_settlements_created_by_admin_user_id",
                table: "transaction_settlements",
                column: "created_by_admin_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_transaction_settlements_admin_users_created_by_admin_user_id",
                table: "transaction_settlements",
                column: "created_by_admin_user_id",
                principalTable: "admin_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_transaction_settlements_financial_transactions_financial_tr~",
                table: "transaction_settlements",
                column: "financial_transaction_id",
                principalTable: "financial_transactions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_transaction_settlements_admin_users_created_by_admin_user_id",
                table: "transaction_settlements");

            migrationBuilder.DropForeignKey(
                name: "FK_transaction_settlements_financial_transactions_financial_tr~",
                table: "transaction_settlements");

            migrationBuilder.DropIndex(
                name: "IX_transaction_settlements_created_by_admin_user_id",
                table: "transaction_settlements");

            migrationBuilder.DropColumn(
                name: "created_by_admin_user_id",
                table: "transaction_settlements");

            migrationBuilder.DropColumn(
                name: "notes",
                table: "transaction_settlements");

            migrationBuilder.DropColumn(
                name: "quantity",
                table: "transaction_settlements");

            migrationBuilder.DropColumn(
                name: "settlement_date",
                table: "transaction_settlements");

            migrationBuilder.DropColumn(
                name: "settlement_mode",
                table: "transaction_settlements");

            migrationBuilder.DropColumn(
                name: "unit_amount",
                table: "transaction_settlements");

            migrationBuilder.RenameColumn(
                name: "financial_transaction_id",
                table: "transaction_settlements",
                newName: "target_transaction_id");

            migrationBuilder.RenameIndex(
                name: "IX_transaction_settlements_financial_transaction_id",
                table: "transaction_settlements",
                newName: "IX_transaction_settlements_target_transaction_id");

            migrationBuilder.AddColumn<int>(
                name: "payment_transaction_id",
                table: "transaction_settlements",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_transaction_settlements_payment_transaction_id",
                table: "transaction_settlements",
                column: "payment_transaction_id");

            migrationBuilder.AddForeignKey(
                name: "FK_transaction_settlements_financial_transactions_payment_tran~",
                table: "transaction_settlements",
                column: "payment_transaction_id",
                principalTable: "financial_transactions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_transaction_settlements_financial_transactions_target_trans~",
                table: "transaction_settlements",
                column: "target_transaction_id",
                principalTable: "financial_transactions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
