using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "libraries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LibraryCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LibraryName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LibraryOwnerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LibraryOwnerNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LibraryOwnerNumber2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LibraryAddress = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    LibraryProvince = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LibraryCity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_libraries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "library_accounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Username = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LibraryCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    POSCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    LibraryId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_library_accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_library_accounts_libraries_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "libraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_library_accounts_LibraryId",
                table: "library_accounts",
                column: "LibraryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "library_accounts");

            migrationBuilder.DropTable(
                name: "libraries");
        }
    }
}
