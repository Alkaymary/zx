using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class AddQrCodesModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "qr_codes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    library_id = table.Column<int>(type: "integer", nullable: false),
                    pos_device_id = table.Column<int>(type: "integer", nullable: false),
                    created_by_library_account_id = table.Column<int>(type: "integer", nullable: false),
                    qr_reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    student_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    student_phone_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    qr_payload = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_qr_codes", x => x.id);
                    table.ForeignKey(
                        name: "FK_qr_codes_libraries_library_id",
                        column: x => x.library_id,
                        principalTable: "libraries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_qr_codes_library_accounts_created_by_library_account_id",
                        column: x => x.created_by_library_account_id,
                        principalTable: "library_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_qr_codes_pos_devices_pos_device_id",
                        column: x => x.pos_device_id,
                        principalTable: "pos_devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_qr_codes_created_by_library_account_id",
                table: "qr_codes",
                column: "created_by_library_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_qr_codes_library_id",
                table: "qr_codes",
                column: "library_id");

            migrationBuilder.CreateIndex(
                name: "IX_qr_codes_pos_device_id",
                table: "qr_codes",
                column: "pos_device_id");

            migrationBuilder.CreateIndex(
                name: "IX_qr_codes_qr_reference",
                table: "qr_codes",
                column: "qr_reference",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "qr_codes");
        }
    }
}
