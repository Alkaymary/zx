using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyApi.Data;
using MyApi.Infrastructure.Security;
using MyApi.Models;

namespace MyApi.IntegrationTests;

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _databaseName = $"myapi-tests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTesting");

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = AuthTestTokens.Issuer,
                ["Jwt:Audience"] = AuthTestTokens.Audience,
                ["Jwt:SecretKey"] = AuthTestTokens.SecretKey,
                ["Jwt:ExpiryMinutes"] = "60",
                ["BootstrapAdmin:Enabled"] = "false",
                ["Database:ApplyMigrationsOnStartup"] = "false",
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Port=5432;Database=ignored;Username=ignored;Password=ignored"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<AppDbContext>));
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<AppDbContext>();

            var inMemoryProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
                options.UseInternalServiceProvider(inMemoryProvider);
            });
        });
    }

    public async Task InitializeAsync()
    {
        using var client = CreateAnonymousClient();
        await SeedAsync();
    }

    public new Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public HttpClient CreateAnonymousClient()
    {
        return CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
    }

    public HttpClient CreateAuthenticatedClient(string bearerToken)
    {
        var client = CreateAnonymousClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        return client;
    }

    private async Task SeedAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (!await dbContext.Libraries.AnyAsync())
        {
            dbContext.Libraries.Add(new Library
            {
                Id = AuthTestTokens.DefaultLibraryId,
                LibraryCode = "LIB-1001",
                LibraryName = "Integration Library",
                Status = RecordStatus.Active
            });
        }

        if (!await dbContext.LibraryAccounts.AnyAsync())
        {
            var officeViewerRole = await dbContext.Roles.FirstAsync(x => x.Code == RoleCodes.OfficeViewer);
            dbContext.LibraryAccounts.Add(new LibraryAccount
            {
                Id = AuthTestTokens.DefaultLibraryAccountId,
                LibraryId = AuthTestTokens.DefaultLibraryId,
                RoleId = officeViewerRole.Id,
                FullName = "Integration Office User",
                Username = "office.test",
                PasswordHash = "test",
                Status = RecordStatus.Active
            });
        }

        if (!await dbContext.AdminUsers.AnyAsync())
        {
            var adminRole = await dbContext.Roles.FirstAsync(x => x.Code == RoleCodes.Admin);
            dbContext.AdminUsers.Add(new AdminUser
            {
                Id = AuthTestTokens.DefaultAdminUserId,
                FullName = "Integration Admin",
                Username = "admin.test",
                PasswordHash = "test",
                RoleId = adminRole.Id,
                Status = RecordStatus.Active
            });
        }

        await dbContext.SaveChangesAsync();
    }
}
