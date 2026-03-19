using MyApi.Models;

namespace MyApi.Services;

public interface IAuditLogSink
{
    bool TryWrite(AuditLog log);
}
