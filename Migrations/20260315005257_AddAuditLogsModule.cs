using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    trace_identifier = table.Column<string>(type: "text", nullable: false),
                    operation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    account_type = table.Column<string>(type: "text", nullable: false),
                    account_id = table.Column<int>(type: "integer", nullable: true),
                    account_username = table.Column<string>(type: "text", nullable: true),
                    role_code = table.Column<string>(type: "text", nullable: true),
                    endpoint = table.Column<string>(type: "text", nullable: false),
                    query_string = table.Column<string>(type: "text", nullable: true),
                    action_name = table.Column<string>(type: "text", nullable: false),
                    http_method = table.Column<string>(type: "text", nullable: false),
                    request_payload = table.Column<string>(type: "text", nullable: true),
                    response_payload = table.Column<string>(type: "text", nullable: true),
                    status_code = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    security_level = table.Column<string>(type: "text", nullable: false),
                    ip_address = table.Column<string>(type: "text", nullable: true),
                    duration_ms = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_account_username",
                table: "audit_logs",
                column: "account_username");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_endpoint",
                table: "audit_logs",
                column: "endpoint");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_operation_date",
                table: "audit_logs",
                column: "operation_date");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_security_level",
                table: "audit_logs",
                column: "security_level");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_status",
                table: "audit_logs",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");
        }
    }
}
