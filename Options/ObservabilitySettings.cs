namespace MyApi.Options;

public sealed class ObservabilitySettings
{
    public const string SectionName = "Observability";

    public bool EnableOpenTelemetry { get; set; }

    public bool EnableConsoleExporter { get; set; }

    public string? ServiceName { get; set; }

    public string? OtlpEndpoint { get; set; }
}
