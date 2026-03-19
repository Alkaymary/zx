namespace MyApi.Models;

public class AuditLog
{
    public int Id { get; set; }

    public string TraceIdentifier { get; set; } = string.Empty;

    public DateTime OperationDate { get; set; } = DateTime.UtcNow;

    public string AccountType { get; set; } = "Anonymous";

    public int? AccountId { get; set; }

    public string? AccountUsername { get; set; }

    public string? RoleCode { get; set; }

    public string Endpoint { get; set; } = string.Empty;

    public string? QueryString { get; set; }

    public string ActionName { get; set; } = string.Empty;

    public string HttpMethod { get; set; } = string.Empty;

    public string? RequestPayload { get; set; }

    public string? ResponsePayload { get; set; }

    public int StatusCode { get; set; }

    public AuditLogStatus Status { get; set; } = AuditLogStatus.Succeeded;

    public AuditSecurityLevel SecurityLevel { get; set; } = AuditSecurityLevel.Public;

    public string? IpAddress { get; set; }

    public long DurationMs { get; set; }
}
