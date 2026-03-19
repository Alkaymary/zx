using Microsoft.Extensions.Configuration;

namespace MyApi.Infrastructure.Secrets;

public sealed class PlaceholderKeyVaultSecretProvider : ISecretProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<PlaceholderKeyVaultSecretProvider> _logger;

    public PlaceholderKeyVaultSecretProvider(
        IConfiguration configuration,
        ILogger<PlaceholderKeyVaultSecretProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string ProviderType => "PlaceholderKeyVault";

    public ValueTask<string?> GetSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        var normalizedKey = secretName.Replace(":", "_", StringComparison.Ordinal);
        var configuredValue = _configuration[$"Secrets:PlaceholderStore:{normalizedKey}"];
        if (!string.IsNullOrWhiteSpace(configuredValue))
        {
            return ValueTask.FromResult<string?>(configuredValue.Trim());
        }

        _logger.LogDebug(
            "No placeholder secret was configured for {SecretName}. Falling back to standard configuration value lookup.",
            secretName);

        configuredValue = _configuration[secretName];
        return ValueTask.FromResult<string?>(string.IsNullOrWhiteSpace(configuredValue) ? null : configuredValue.Trim());
    }
}
