using MyApi.Models;

namespace MyApi.Services;

public interface IJwtTokenService
{
    string CreateAdminToken(AdminUser adminUser);

    string CreateLibraryAccountToken(LibraryAccount libraryAccount);
}
