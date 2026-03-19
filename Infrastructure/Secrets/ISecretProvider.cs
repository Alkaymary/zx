namespace MyApi.Infrastructure.Secrets;

public interface ISecretProvider
{
    string ProviderType { get; }

    ValueTask<string?> GetSecretAsync(string secretName, CancellationToken cancellationToken = default);
}
