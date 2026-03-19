using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class AlignCoreSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_library_accounts_libraries_LibraryId",
                table: "library_accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_pos_devices_libraries_LibraryId",
                table: "pos_devices");

            migrationBuilder.DropForeignKey(
                name: "FK_pos_devices_library_accounts_ActivatedByAccountId",
                table: "pos_devices");

            migrationBuilder.DropColumn(
                name: "LibraryCode",
                table: "library_accounts");

            migrationBuilder.DropColumn(
                name: "POSCode",
                table: "library_accounts");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "pos_devices",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "pos_devices",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "SerialNumber",
                table: "pos_devices",
                newName: "serial_number");

            migrationBuilder.RenameColumn(
                name: "POSCode",
                table: "pos_devices",
                newName: "pos_code");

            migrationBuilder.RenameColumn(
                name: "LibraryId",
                table: "pos_devices",
                newName: "library_id");

            migrationBuilder.RenameColumn(
                name: "LastAuthenticatedAt",
                table: "pos_devices",
                newName: "last_authenticated_at");

            migrationBuilder.RenameColumn(
                name: "IsActivated",
                table: "pos_devices",
                newName: "is_activated");

            migrationBuilder.RenameColumn(
                name: "DeviceVendor",
                table: "pos_devices",
                newName: "device_vendor");

            migrationBuilder.RenameColumn(
                name: "DeviceModel",
                table: "pos_devices",
                newName: "device_model");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "pos_devices",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ActivationToken",
                table: "pos_devices",
                newName: "activation_token");

            migrationBuilder.RenameColumn(
                name: "ActivatedByAccountId",
                table: "pos_devices",
                newName: "activated_by_account_id");

            migrationBuilder.RenameColumn(
                name: "ActivatedAt",
                table: "pos_devices",
                newName: "activated_at");

            migrationBuilder.RenameIndex(
                name: "IX_pos_devices_LibraryId",
                table: "pos_devices",
                newName: "IX_pos_devices_library_id");

            migrationBuilder.RenameIndex(
                name: "IX_pos_devices_ActivatedByAccountId",
                table: "pos_devices",
                newName: "IX_pos_devices_activated_by_account_id");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "library_accounts",
                newName: "username");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "library_accounts",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "library_accounts",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "library_accounts",
                newName: "phone_number");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "library_accounts",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "LibraryId",
                table: "library_accounts",
                newName: "library_id");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "library_accounts",
                newName: "full_name");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "library_accounts",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_library_accounts_LibraryId",
                table: "library_accounts",
                newName: "IX_library_accounts_library_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "libraries",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "LibraryName",
                table: "libraries",
                newName: "library_name");

            migrationBuilder.RenameColumn(
                name: "LibraryCode",
                table: "libraries",
                newName: "library_code");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "libraries",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LibraryProvince",
                table: "libraries",
                newName: "province");

            migrationBuilder.RenameColumn(
                name: "LibraryOwnerNumber2",
                table: "libraries",
                newName: "owner_phone_2");

            migrationBuilder.RenameColumn(
                name: "LibraryOwnerNumber",
                table: "libraries",
                newName: "owner_phone");

            migrationBuilder.RenameColumn(
                name: "LibraryOwnerName",
                table: "libraries",
                newName: "owner_name");

            migrationBuilder.RenameColumn(
                name: "LibraryCity",
                table: "libraries",
                newName: "city");

            migrationBuilder.RenameColumn(
                name: "LibraryAddress",
                table: "libraries",
                newName: "address");

            migrationBuilder.Sql("""
                ALTER TABLE pos_devices
                ALTER COLUMN status TYPE text
                USING CASE
                    WHEN status = 0 THEN 'active'
                    WHEN status = 1 THEN 'inactive'
                    WHEN status = 2 THEN 'suspended'
                    WHEN status = 3 THEN 'maintenance'
                    ELSE 'inactive'
                END;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "pos_devices",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "pos_code",
                table: "pos_devices",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "pos_devices",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<string>(
                name: "username",
                table: "library_accounts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.Sql("""
                ALTER TABLE library_accounts
                ALTER COLUMN status TYPE text
                USING CASE
                    WHEN status = 0 THEN 'active'
                    WHEN status = 1 THEN 'inactive'
                    WHEN status = 2 THEN 'suspended'
                    ELSE 'active'
                END;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "library_accounts",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "password_hash",
                table: "library_accounts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "library_id",
                table: "library_accounts",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "full_name",
                table: "library_accounts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_login_at",
                table: "library_accounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "role_id",
                table: "library_accounts",
                type: "integer",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "library_accounts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<string>(
                name: "library_name",
                table: "libraries",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "library_code",
                table: "libraries",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "credit_limit",
                table: "libraries",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "current_balance",
                table: "libraries",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "latitude",
                table: "libraries",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "longitude",
                table: "libraries",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "libraries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "libraries",
                type: "text",
                nullable: false,
                defaultValue: "active");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "libraries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    guard_name = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "name", "code", "guard_name" },
                values: new object[,]
                {
                    { 1, "Super Admin", "super_admin", "admin" },
                    { 2, "Admin", "admin", "admin" },
                    { 3, "Accountant", "accountant", "admin" },
                    { 4, "Office Manager", "office_manager", "office" },
                    { 5, "Office User", "office_user", "office" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_pos_devices_pos_code",
                table: "pos_devices",
                column: "pos_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_library_accounts_role_id",
                table: "library_accounts",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_library_accounts_username",
                table: "library_accounts",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_libraries_library_code",
                table: "libraries",
                column: "library_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_code",
                table: "roles",
                column: "code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_library_accounts_libraries_library_id",
                table: "library_accounts",
                column: "library_id",
                principalTable: "libraries",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_library_accounts_roles_role_id",
                table: "library_accounts",
                column: "role_id",
                principalTable: "roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_pos_devices_libraries_library_id",
                table: "pos_devices",
                column: "library_id",
                principalTable: "libraries",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_pos_devices_library_accounts_activated_by_account_id",
                table: "pos_devices",
                column: "activated_by_account_id",
                principalTable: "library_accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_library_accounts_libraries_library_id",
                table: "library_accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_library_accounts_roles_role_id",
                table: "library_accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_pos_devices_libraries_library_id",
                table: "pos_devices");

            migrationBuilder.DropForeignKey(
                name: "FK_pos_devices_library_accounts_activated_by_account_id",
                table: "pos_devices");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropIndex(
                name: "IX_pos_devices_pos_code",
                table: "pos_devices");

            migrationBuilder.DropIndex(
                name: "IX_library_accounts_role_id",
                table: "library_accounts");

            migrationBuilder.DropIndex(
                name: "IX_library_accounts_username",
                table: "library_accounts");

            migrationBuilder.DropIndex(
                name: "IX_libraries_library_code",
                table: "libraries");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "pos_devices");

            migrationBuilder.DropColumn(
                name: "last_login_at",
                table: "library_accounts");

            migrationBuilder.DropColumn(
                name: "role_id",
                table: "library_accounts");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "library_accounts");

            migrationBuilder.DropColumn(
                name: "credit_limit",
                table: "libraries");

            migrationBuilder.DropColumn(
                name: "current_balance",
                table: "libraries");

            migrationBuilder.DropColumn(
                name: "latitude",
                table: "libraries");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "libraries");

            migrationBuilder.DropColumn(
                name: "notes",
                table: "libraries");

            migrationBuilder.DropColumn(
                name: "status",
                table: "libraries");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "libraries");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "pos_devices",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "pos_devices",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "serial_number",
                table: "pos_devices",
                newName: "SerialNumber");

            migrationBuilder.RenameColumn(
                name: "pos_code",
                table: "pos_devices",
                newName: "POSCode");

            migrationBuilder.RenameColumn(
                name: "library_id",
                table: "pos_devices",
                newName: "LibraryId");

            migrationBuilder.RenameColumn(
                name: "last_authenticated_at",
                table: "pos_devices",
                newName: "LastAuthenticatedAt");

            migrationBuilder.RenameColumn(
                name: "is_activated",
                table: "pos_devices",
                newName: "IsActivated");

            migrationBuilder.RenameColumn(
                name: "device_vendor",
                table: "pos_devices",
                newName: "DeviceVendor");

            migrationBuilder.RenameColumn(
                name: "device_model",
                table: "pos_devices",
                newName: "DeviceModel");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "pos_devices",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "activation_token",
                table: "pos_devices",
                newName: "ActivationToken");

            migrationBuilder.RenameColumn(
                name: "activated_by_account_id",
                table: "pos_devices",
                newName: "ActivatedByAccountId");

            migrationBuilder.RenameColumn(
                name: "activated_at",
                table: "pos_devices",
                newName: "ActivatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_pos_devices_library_id",
                table: "pos_devices",
                newName: "IX_pos_devices_LibraryId");

            migrationBuilder.RenameIndex(
                name: "IX_pos_devices_activated_by_account_id",
                table: "pos_devices",
                newName: "IX_pos_devices_ActivatedByAccountId");

            migrationBuilder.RenameColumn(
                name: "username",
                table: "library_accounts",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "library_accounts",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "library_accounts",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "phone_number",
                table: "library_accounts",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "library_accounts",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "library_id",
                table: "library_accounts",
                newName: "LibraryId");

            migrationBuilder.RenameColumn(
                name: "full_name",
                table: "library_accounts",
                newName: "FullName");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "library_accounts",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_library_accounts_library_id",
                table: "library_accounts",
                newName: "IX_library_accounts_LibraryId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "libraries",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "library_name",
                table: "libraries",
                newName: "LibraryName");

            migrationBuilder.RenameColumn(
                name: "library_code",
                table: "libraries",
                newName: "LibraryCode");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "libraries",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "province",
                table: "libraries",
                newName: "LibraryProvince");

            migrationBuilder.RenameColumn(
                name: "owner_phone_2",
                table: "libraries",
                newName: "LibraryOwnerNumber2");

            migrationBuilder.RenameColumn(
                name: "owner_phone",
                table: "libraries",
                newName: "LibraryOwnerNumber");

            migrationBuilder.RenameColumn(
                name: "owner_name",
                table: "libraries",
                newName: "LibraryOwnerName");

            migrationBuilder.RenameColumn(
                name: "city",
                table: "libraries",
                newName: "LibraryCity");

            migrationBuilder.RenameColumn(
                name: "address",
                table: "libraries",
                newName: "LibraryAddress");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "pos_devices",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "POSCode",
                table: "pos_devices",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "library_accounts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "library_accounts",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "library_accounts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<int>(
                name: "LibraryId",
                table: "library_accounts",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "library_accounts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "LibraryCode",
                table: "library_accounts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "POSCode",
                table: "library_accounts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LibraryName",
                table: "libraries",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "LibraryCode",
                table: "libraries",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddForeignKey(
                name: "FK_library_accounts_libraries_LibraryId",
                table: "library_accounts",
                column: "LibraryId",
                principalTable: "libraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_pos_devices_libraries_LibraryId",
                table: "pos_devices",
                column: "LibraryId",
                principalTable: "libraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_pos_devices_library_accounts_ActivatedByAccountId",
                table: "pos_devices",
                column: "ActivatedByAccountId",
                principalTable: "library_accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
