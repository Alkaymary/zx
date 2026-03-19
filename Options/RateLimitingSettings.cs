namespace MyApi.Options;

public sealed class RateLimitingSettings
{
    public const string SectionName = "RateLimiting";

    public FixedWindowRateLimitSettings Login { get; set; } = new();

    public FixedWindowRateLimitSettings Refresh { get; set; } = new()
    {
        PermitLimit = 10
    };
}

public sealed class FixedWindowRateLimitSettings
{
    public int PermitLimit { get; set; } = 5;

    public int WindowSeconds { get; set; } = 60;

    public int QueueLimit { get; set; } = 0;
}
