namespace MyApi.Options;

public sealed class SecretsSettings
{
    public const string SectionName = "Secrets";

    public string ProviderType { get; set; } = "Environment";
}
