using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "financial_transactions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    library_id = table.Column<int>(type: "integer", nullable: false),
                    transaction_type = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    paid_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    remaining_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    transaction_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_by_admin_user_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_financial_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_financial_transactions_admin_users_created_by_admin_user_id",
                        column: x => x.created_by_admin_user_id,
                        principalTable: "admin_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_financial_transactions_libraries_library_id",
                        column: x => x.library_id,
                        principalTable: "libraries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transaction_settlements",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    payment_transaction_id = table.Column<int>(type: "integer", nullable: false),
                    target_transaction_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction_settlements", x => x.id);
                    table.ForeignKey(
                        name: "FK_transaction_settlements_financial_transactions_payment_tran~",
                        column: x => x.payment_transaction_id,
                        principalTable: "financial_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_transaction_settlements_financial_transactions_target_trans~",
                        column: x => x.target_transaction_id,
                        principalTable: "financial_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_financial_transactions_created_by_admin_user_id",
                table: "financial_transactions",
                column: "created_by_admin_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_financial_transactions_library_id",
                table: "financial_transactions",
                column: "library_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_settlements_payment_transaction_id",
                table: "transaction_settlements",
                column: "payment_transaction_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_settlements_target_transaction_id",
                table: "transaction_settlements",
                column: "target_transaction_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transaction_settlements");

            migrationBuilder.DropTable(
                name: "financial_transactions");
        }
    }
}
