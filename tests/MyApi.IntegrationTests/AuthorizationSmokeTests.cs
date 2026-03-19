using System.Net;
using System.Net.Http.Json;
using MyApi.Infrastructure.Security;

namespace MyApi.IntegrationTests;

public sealed class AuthorizationSmokeTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AuthorizationSmokeTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Admin_users_without_token_returns_401()
    {
        using var client = _factory.CreateAnonymousClient();

        var response = await client.GetAsync("/api/AdminUsers");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Admin_users_with_library_account_token_returns_403()
    {
        using var client = _factory.CreateAuthenticatedClient(AuthTestTokens.CreateLibraryAccountToken());

        var response = await client.GetAsync("/api/AdminUsers");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Packages_read_with_viewer_token_returns_200()
    {
        using var client = _factory.CreateAuthenticatedClient(AuthTestTokens.CreateAdminToken(RoleCodes.Viewer));

        var response = await client.GetAsync("/api/Packages");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Financial_write_with_admin_token_returns_403()
    {
        using var client = _factory.CreateAuthenticatedClient(AuthTestTokens.CreateAdminToken(RoleCodes.Admin));

        var response = await client.PostAsJsonAsync("/api/FinancialTransactions", new
        {
            libraryId = AuthTestTokens.DefaultLibraryId,
            amount = 15000,
            description = "integration test",
            transactionDate = DateTime.UtcNow,
            dueDate = DateTime.UtcNow.AddDays(7)
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Pos_write_with_support_token_returns_201()
    {
        using var client = _factory.CreateAuthenticatedClient(AuthTestTokens.CreateAdminToken(RoleCodes.Support));

        var response = await client.PostAsJsonAsync("/api/PosDevices", new
        {
            libraryId = AuthTestTokens.DefaultLibraryId,
            posCode = "POS-TEST-01",
            serialNumber = "SER-TEST-01",
            deviceModel = "Model X",
            deviceVendor = "Vendor Y",
            status = "Active",
            isActivated = false
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Library_financial_with_admin_token_returns_403()
    {
        using var client = _factory.CreateAuthenticatedClient(AuthTestTokens.CreateAdminToken(RoleCodes.Admin));

        var response = await client.GetAsync("/api/LibraryFinancial/me/summary");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Library_financial_with_library_account_token_returns_200()
    {
        using var client = _factory.CreateAuthenticatedClient(AuthTestTokens.CreateLibraryAccountToken());

        var response = await client.GetAsync("/api/LibraryFinancial/me/summary");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Expired_token_returns_401()
    {
        var expiredToken = AuthTestTokens.CreateAdminToken(RoleCodes.Viewer, expiresAt: DateTime.UtcNow.AddMinutes(-10));
        using var client = _factory.CreateAuthenticatedClient(expiredToken);

        var response = await client.GetAsync("/api/Packages");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
