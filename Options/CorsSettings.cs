namespace MyApi.Options;

public sealed class CorsSettings
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; set; } = [];

    public bool AllowLocalhostOriginsInDevelopment { get; set; } = true;
}
