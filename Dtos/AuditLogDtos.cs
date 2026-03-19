using MyApi.Models;

namespace MyApi.Dtos;

public class AuditLogQueryDto
{
    public int? Id { get; set; }

    public string? Search { get; set; }

    public string? Endpoint { get; set; }

    public string? HttpMethod { get; set; }

    public AuditLogStatus? Status { get; set; }

    public AuditSecurityLevel? SecurityLevel { get; set; }

    public string? AccountUsername { get; set; }

    public string? IpAddress { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 25;
}

public class AuditLogListItemDto
{
    public int Id { get; set; }

    public DateTime OperationDate { get; set; }

    public string TraceIdentifier { get; set; } = string.Empty;

    public string AccountType { get; set; } = string.Empty;

    public int? AccountId { get; set; }

    public string? AccountUsername { get; set; }

    public string? RoleCode { get; set; }

    public string Endpoint { get; set; } = string.Empty;

    public string? QueryString { get; set; }

    public string ActionName { get; set; } = string.Empty;

    public string HttpMethod { get; set; } = string.Empty;

    public int StatusCode { get; set; }

    public AuditLogStatus Status { get; set; }

    public AuditSecurityLevel SecurityLevel { get; set; }

    public string? IpAddress { get; set; }

    public long DurationMs { get; set; }
}

public class AuditLogDetailDto : AuditLogListItemDto
{
    public string? RequestPayload { get; set; }

    public string? ResponsePayload { get; set; }
}

public class AuditLogPagedResponseDto
{
    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }

    public List<AuditLogListItemDto> Items { get; set; } = [];
}

public class AuditLogMetricsDto
{
    public int TotalLogs { get; set; }

    public int TodayLogs { get; set; }

    public int SuccessfulLogs { get; set; }

    public int ClientErrors { get; set; }

    public int BlockedLogs { get; set; }

    public int ServerErrors { get; set; }

    public int PublicLogs { get; set; }

    public int ProtectedLogs { get; set; }

    public int PrivilegedLogs { get; set; }

    public int AdminLogs { get; set; }

    public int LibraryLogs { get; set; }
}
