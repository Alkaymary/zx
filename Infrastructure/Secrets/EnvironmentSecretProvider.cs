using Microsoft.Extensions.Configuration;

namespace MyApi.Infrastructure.Secrets;

public sealed class EnvironmentSecretProvider : ISecretProvider
{
    private static readonly IReadOnlyDictionary<string, string[]> SecretEnvironmentMappings =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [SecretNames.DefaultConnectionString] =
            [
                "MYAPI_CONNECTION_STRING",
                "ConnectionStrings__DefaultConnection"
            ],
            [SecretNames.JwtSecretKey] =
            [
                "MYAPI_JWT_SECRET",
                "Jwt__SecretKey"
            ],
            [SecretNames.BootstrapAdminPassword] =
            [
                "MYAPI_BOOTSTRAP_ADMIN_PASSWORD",
                "BootstrapAdmin__Password"
            ]
        };

    private readonly IConfiguration _configuration;

    public EnvironmentSecretProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string ProviderType => "Environment";

    public ValueTask<string?> GetSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        if (SecretEnvironmentMappings.TryGetValue(secretName, out var environmentVariableNames))
        {
            foreach (var environmentVariableName in environmentVariableNames)
            {
                var environmentValue = Environment.GetEnvironmentVariable(environmentVariableName);
                if (!string.IsNullOrWhiteSpace(environmentValue))
                {
                    return ValueTask.FromResult<string?>(environmentValue.Trim());
                }
            }
        }

        var configuredValue = _configuration[secretName];
        return ValueTask.FromResult<string?>(string.IsNullOrWhiteSpace(configuredValue) ? null : configuredValue.Trim());
    }
}
