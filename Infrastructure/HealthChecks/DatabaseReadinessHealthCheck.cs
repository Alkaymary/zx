using Microsoft.Extensions.Diagnostics.HealthChecks;
using MyApi.Data;

namespace MyApi.Infrastructure.HealthChecks;

public sealed class DatabaseReadinessHealthCheck : IHealthCheck
{
    private readonly AppDbContext _dbContext;

    public DatabaseReadinessHealthCheck(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(TimeSpan.FromSeconds(5));

        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(timeoutSource.Token);
            return canConnect
                ? HealthCheckResult.Healthy("Database connection is available.")
                : HealthCheckResult.Unhealthy("Database is unavailable.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy("Database readiness check timed out.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database is unavailable.", ex);
        }
    }
}
