using System.Threading.Channels;
using MyApi.Models;

namespace MyApi.Services;

public sealed class QueuedAuditLogSink : IAuditLogSink
{
    private const int Capacity = 500;
    private readonly Channel<AuditLog> _channel;
    private readonly ILogger<QueuedAuditLogSink> _logger;

    public QueuedAuditLogSink(ILogger<QueuedAuditLogSink> logger)
    {
        _logger = logger;
        _channel = Channel.CreateBounded<AuditLog>(new BoundedChannelOptions(Capacity)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        });
    }

    public ChannelReader<AuditLog> Reader => _channel.Reader;

    public bool TryWrite(AuditLog log)
    {
        var written = _channel.Writer.TryWrite(log);
        if (!written)
        {
            _logger.LogWarning(
                "Dropping audit log for trace {TraceIdentifier} because the queue is full.",
                log.TraceIdentifier);
        }

        return written;
    }
}
