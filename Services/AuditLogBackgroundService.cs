using Microsoft.EntityFrameworkCore;
using MyApi.Data;

namespace MyApi.Services;

public sealed class AuditLogBackgroundService : BackgroundService
{
    private readonly QueuedAuditLogSink _auditLogSink;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditLogBackgroundService> _logger;

    public AuditLogBackgroundService(
        QueuedAuditLogSink auditLogSink,
        IServiceScopeFactory scopeFactory,
        ILogger<AuditLogBackgroundService> logger)
    {
        _auditLogSink = auditLogSink;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var reader = _auditLogSink.Reader;

        try
        {
            while (await reader.WaitToReadAsync(stoppingToken))
            {
                while (reader.TryRead(out var auditLog))
                {
                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        dbContext.AuditLogs.Add(auditLog);
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        return;
                    }
                    catch (DbUpdateException exception)
                    {
                        _logger.LogWarning(exception, "Audit log persistence failed for trace {TraceIdentifier}.", auditLog.TraceIdentifier);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogWarning(exception, "Audit log persistence failed for trace {TraceIdentifier}.", auditLog.TraceIdentifier);
                    }
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }
}
