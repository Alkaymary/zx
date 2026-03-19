using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class HardenAuditLogIndexesAndPgTrgm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.Sql(@"CREATE EXTENSION IF NOT EXISTS pg_trgm;");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_action_name",
                table: "audit_logs",
                column: "action_name");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_ip_address",
                table: "audit_logs",
                column: "ip_address");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_operation_date_id",
                table: "audit_logs",
                columns: new[] { "operation_date", "id" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_trace_identifier",
                table: "audit_logs",
                column: "trace_identifier");

            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_audit_logs_endpoint_trgm"" ON audit_logs USING GIN (endpoint gin_trgm_ops);");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_audit_logs_action_name_trgm"" ON audit_logs USING GIN (action_name gin_trgm_ops);");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_audit_logs_account_username_trgm"" ON audit_logs USING GIN (account_username gin_trgm_ops);");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_audit_logs_ip_address_trgm"" ON audit_logs USING GIN (ip_address gin_trgm_ops);");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_audit_logs_trace_identifier_trgm"" ON audit_logs USING GIN (trace_identifier gin_trgm_ops);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_audit_logs_trace_identifier_trgm"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_audit_logs_ip_address_trgm"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_audit_logs_account_username_trgm"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_audit_logs_action_name_trgm"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_audit_logs_endpoint_trgm"";");

            migrationBuilder.DropIndex(
                name: "IX_audit_logs_action_name",
                table: "audit_logs");

            migrationBuilder.DropIndex(
                name: "IX_audit_logs_ip_address",
                table: "audit_logs");

            migrationBuilder.DropIndex(
                name: "IX_audit_logs_operation_date_id",
                table: "audit_logs");

            migrationBuilder.DropIndex(
                name: "IX_audit_logs_trace_identifier",
                table: "audit_logs");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,");
        }
    }
}
