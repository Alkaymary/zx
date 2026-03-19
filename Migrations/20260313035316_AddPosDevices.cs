using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPosDevices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pos_devices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    POSCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SerialNumber = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    DeviceModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeviceVendor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsActivated = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastAuthenticatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LibraryId = table.Column<int>(type: "integer", nullable: true),
                    ActivatedByAccountId = table.Column<int>(type: "integer", nullable: true),
                    ActivationToken = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pos_devices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pos_devices_libraries_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "libraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_pos_devices_library_accounts_ActivatedByAccountId",
                        column: x => x.ActivatedByAccountId,
                        principalTable: "library_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_pos_devices_ActivatedByAccountId",
                table: "pos_devices",
                column: "ActivatedByAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_pos_devices_LibraryId",
                table: "pos_devices",
                column: "LibraryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pos_devices");
        }
    }
}
