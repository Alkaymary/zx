using Microsoft.EntityFrameworkCore;
using MyApi.Models;

namespace MyApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<Library> Libraries => Set<Library>();
    public DbSet<LibraryAccount> LibraryAccounts => Set<LibraryAccount>();
    public DbSet<PosDevice> PosDevices => Set<PosDevice>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Package> Packages => Set<Package>();
    public DbSet<FinancialTransaction> FinancialTransactions => Set<FinancialTransaction>();
    public DbSet<TransactionSettlement> TransactionSettlements => Set<TransactionSettlement>();
    public DbSet<QrCode> QrCodes => Set<QrCode>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresExtension("pg_trgm");

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(x => x.Name).HasColumnName("name");
            entity.Property(x => x.Code).HasColumnName("code");
            entity.Property(x => x.GuardName)
                .HasColumnName("guard_name")
                .ToLowerCaseString();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.ToTable("admin_users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(x => x.FullName).HasColumnName("full_name");
            entity.Property(x => x.Username).HasColumnName("username");
            entity.Property(x => x.Email).HasColumnName("email");
            entity.Property(x => x.PhoneNumber).HasColumnName("phone_number");
            entity.Property(x => x.PasswordHash).HasColumnName("password_hash");
            entity.Property(x => x.RoleId).HasColumnName("role_id");
            entity.Property(x => x.Status)
                .HasColumnName("status")
                .ToLowerCaseString();
            entity.Property(x => x.LastLoginAt).HasColumnName("last_login_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(x => x.Username).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasOne(x => x.Role)
                .WithMany(x => x.AdminUsers)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Library>(entity =>
        {
            entity.ToTable("libraries");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(x => x.LibraryCode).HasColumnName("library_code");
            entity.Property(x => x.LibraryName).HasColumnName("library_name");
            entity.Property(x => x.OwnerName).HasColumnName("owner_name");
            entity.Property(x => x.OwnerPhone).HasColumnName("owner_phone");
            entity.Property(x => x.OwnerPhone2).HasColumnName("owner_phone_2");
            entity.Property(x => x.Address).HasColumnName("address");
            entity.Property(x => x.Province).HasColumnName("province");
            entity.Property(x => x.City).HasColumnName("city");
            entity.Property(x => x.Latitude).HasColumnName("latitude");
            entity.Property(x => x.Longitude).HasColumnName("longitude");
            entity.Property(x => x.Status)
                .HasColumnName("status")
                .ToLowerCaseString();
            entity.Property(x => x.CreditLimit).HasColumnName("credit_limit");
            entity.Property(x => x.CurrentBalance).HasColumnName("current_balance");
            entity.Property(x => x.Notes).HasColumnName("notes");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(x => x.LibraryCode).IsUnique();
            entity.HasMany(x => x.Accounts)
                .WithOne(x => x.Library)
                .HasForeignKey(x => x.LibraryId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.PosDevices)
                .WithOne(x => x.Library)
                .HasForeignKey(x => x.LibraryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<LibraryAccount>(entity =>
        {
            entity.ToTable("library_accounts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(x => x.LibraryId).HasColumnName("library_id");
            entity.Property(x => x.RoleId).HasColumnName("role_id");
            entity.Property(x => x.FullName).HasColumnName("full_name");
            entity.Property(x => x.Username).HasColumnName("username");
            entity.Property(x => x.PhoneNumber).HasColumnName("phone_number");
            entity.Property(x => x.PasswordHash).HasColumnName("password_hash");
            entity.Property(x => x.Status)
                .HasColumnName("status")
                .ToLowerCaseString();
            entity.Property(x => x.LastLoginAt).HasColumnName("last_login_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(x => x.Username).IsUnique();
            entity.HasOne(x => x.Role)
                .WithMany(x => x.LibraryAccounts)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.ActivatedDevices)
                .WithOne(x => x.ActivatedByAccount)
                .HasForeignKey(x => x.ActivatedByAccountId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PosDevice>(entity =>
        {
            entity.ToTable("pos_devices");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(x => x.LibraryId).HasColumnName("library_id");
            entity.Property(x => x.PosCode).HasColumnName("pos_code");
            entity.Property(x => x.SerialNumber).HasColumnName("serial_number");
            entity.Property(x => x.DeviceModel).HasColumnName("device_model");
            entity.Property(x => x.DeviceVendor).HasColumnName("device_vendor");
            entity.Property(x => x.Status)
                .HasColumnName("status")
                .ToLowerCaseString();
            entity.Property(x => x.IsActivated).HasColumnName("is_activated");
            entity.Property(x => x.ActivatedByAccountId).HasColumnName("activated_by_account_id");
            entity.Property(x => x.ActivationToken).HasColumnName("activation_token");
            entity.Property(x => x.ActivatedAt).HasColumnName("activated_at");
            entity.Property(x => x.LastAuthenticatedAt).HasColumnName("last_authenticated_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(x => x.PosCode).IsUnique();
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(x => x.Token).HasColumnName("token");
            entity.Property(x => x.AdminUserId).HasColumnName("admin_user_id");
            entity.Property(x => x.LibraryAccountId).HasColumnName("library_account_id");
            entity.Property(x => x.ExpiresAt).HasColumnName("expires_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.RevokedAt).HasColumnName("revoked_at");
            entity.Property(x => x.UserType).HasColumnName("user_type");
            entity.HasIndex(x => x.Token).IsUnique();
            entity.HasOne(x => x.AdminUser)
                .WithMany()
                .HasForeignKey(x => x.AdminUserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.LibraryAccount)
                .WithMany()
                .HasForeignKey(x => x.LibraryAccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Package>(entity =>
        {
            entity.ToTable("packages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(x => x.PackageCode).HasColumnName("package_code");
            entity.Property(x => x.Name).HasColumnName("name");
            entity.Property(x => x.PriceIqd).HasColumnName("price_iqd");
            entity.Property(x => x.Status)
                .HasColumnName("status")
                .ToLowerCaseString();
            entity.Property(x => x.AddedByAdminUserId).HasColumnName("added_by_admin_user_id");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(x => x.PackageCode).IsUnique();
            entity.HasIndex(x => x.Name).IsUnique();
            entity.HasOne(x => x.AddedByAdminUser)
                .WithMany()
                .HasForeignKey(x => x.AddedByAdminUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FinancialTransaction>(entity =>
        {
            entity.ToTable("financial_transactions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(x => x.LibraryId).HasColumnName("library_id");
            entity.Property(x => x.TransactionType)
                .HasColumnName("transaction_type")
                .ToLowerCaseString();
            entity.Property(x => x.Amount).HasColumnName("amount");
            entity.Property(x => x.PaidAmount).HasColumnName("paid_amount");
            entity.Property(x => x.RemainingAmount).HasColumnName("remaining_amount");
            entity.Property(x => x.Description).HasColumnName("description");
            entity.Property(x => x.TransactionDate).HasColumnName("transaction_date");
            entity.Property(x => x.DueDate).HasColumnName("due_date");
            entity.Property(x => x.Status)
                .HasColumnName("status")
                .ToLowerCaseString();
            entity.Property(x => x.CreatedByAdminUserId).HasColumnName("created_by_admin_user_id");
            entity.Property(x => x.CreatedByLibraryAccountId).HasColumnName("created_by_library_account_id");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne(x => x.Library)
                .WithMany()
                .HasForeignKey(x => x.LibraryId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.CreatedByAdminUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByAdminUserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CreatedByLibraryAccount)
                .WithMany()
                .HasForeignKey(x => x.CreatedByLibraryAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TransactionSettlement>(entity =>
        {
            entity.ToTable("transaction_settlements");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(x => x.FinancialTransactionId).HasColumnName("financial_transaction_id");
            entity.Property(x => x.SettlementMode)
                .HasColumnName("settlement_mode")
                .ToLowerCaseString();
            entity.Property(x => x.Quantity).HasColumnName("quantity");
            entity.Property(x => x.UnitAmount).HasColumnName("unit_amount");
            entity.Property(x => x.Amount).HasColumnName("amount");
            entity.Property(x => x.SettlementDate).HasColumnName("settlement_date");
            entity.Property(x => x.CreatedByAdminUserId).HasColumnName("created_by_admin_user_id");
            entity.Property(x => x.Notes).HasColumnName("notes");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne(x => x.FinancialTransaction)
                .WithMany(x => x.Settlements)
                .HasForeignKey(x => x.FinancialTransactionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.CreatedByAdminUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByAdminUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<QrCode>(entity =>
        {
            entity.ToTable("qr_codes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(x => x.LibraryId).HasColumnName("library_id");
            entity.Property(x => x.PackageId).HasColumnName("package_id");
            entity.Property(x => x.PosDeviceId).HasColumnName("pos_device_id");
            entity.Property(x => x.CreatedByLibraryAccountId).HasColumnName("created_by_library_account_id");
            entity.Property(x => x.FinancialTransactionId).HasColumnName("financial_transaction_id");
            entity.Property(x => x.QrReference).HasColumnName("qr_reference");
            entity.Property(x => x.StudentName).HasColumnName("student_name");
            entity.Property(x => x.StudentPhoneNumber).HasColumnName("student_phone_number");
            entity.Property(x => x.QrPayload).HasColumnName("qr_payload");
            entity.Property(x => x.Status)
                .HasColumnName("status")
                .ToLowerCaseString();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(x => x.QrReference).IsUnique();
            entity.HasOne(x => x.Library)
                .WithMany()
                .HasForeignKey(x => x.LibraryId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Package)
                .WithMany()
                .HasForeignKey(x => x.PackageId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PosDevice)
                .WithMany()
                .HasForeignKey(x => x.PosDeviceId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CreatedByLibraryAccount)
                .WithMany()
                .HasForeignKey(x => x.CreatedByLibraryAccountId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.FinancialTransaction)
                .WithMany(x => x.QrCodes)
                .HasForeignKey(x => x.FinancialTransactionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(x => x.TraceIdentifier).HasColumnName("trace_identifier");
            entity.Property(x => x.OperationDate).HasColumnName("operation_date").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.AccountType).HasColumnName("account_type");
            entity.Property(x => x.AccountId).HasColumnName("account_id");
            entity.Property(x => x.AccountUsername).HasColumnName("account_username");
            entity.Property(x => x.RoleCode).HasColumnName("role_code");
            entity.Property(x => x.Endpoint).HasColumnName("endpoint");
            entity.Property(x => x.QueryString).HasColumnName("query_string");
            entity.Property(x => x.ActionName).HasColumnName("action_name");
            entity.Property(x => x.HttpMethod).HasColumnName("http_method");
            entity.Property(x => x.RequestPayload).HasColumnName("request_payload").HasColumnType("text");
            entity.Property(x => x.ResponsePayload).HasColumnName("response_payload").HasColumnType("text");
            entity.Property(x => x.StatusCode).HasColumnName("status_code");
            entity.Property(x => x.Status)
                .HasColumnName("status")
                .ToLowerCaseString();
            entity.Property(x => x.SecurityLevel)
                .HasColumnName("security_level")
                .ToLowerCaseString();
            entity.Property(x => x.IpAddress).HasColumnName("ip_address");
            entity.Property(x => x.DurationMs).HasColumnName("duration_ms");
            entity.HasIndex(x => x.OperationDate);
            entity.HasIndex(x => new { x.OperationDate, x.Id }).IsDescending(true, true);
            entity.HasIndex(x => x.Endpoint);
            entity.HasIndex(x => x.ActionName);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.SecurityLevel);
            entity.HasIndex(x => x.AccountUsername);
            entity.HasIndex(x => x.IpAddress);
            entity.HasIndex(x => x.TraceIdentifier);
        });
    }
}

internal static class EnumPropertyBuilderExtensions
{
    public static Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder<TEnum> ToLowerCaseString<TEnum>(
        this Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder<TEnum> propertyBuilder)
        where TEnum : struct, Enum
    {
        return propertyBuilder.HasConversion(
            value => value.ToString().ToLowerInvariant(),
            value => Enum.Parse<TEnum>(value, true));
    }
}
