namespace MyApi.Models;

public enum GuardName
{
    Admin,
    Office
}

public enum RecordStatus
{
    Active,
    Used,
    Inactive,
    Suspended
}

public enum PosDeviceStatus
{
    Active,
    Inactive,
    Suspended,
    Maintenance
}

public enum FinancialTransactionType
{
    OpenInvoice
}

public enum FinancialTransactionStatus
{
    Open,
    PartiallyPaid,
    Paid,
    Cancelled
}

public enum FinancialSettlementMode
{
    Full,
    PartialAmount,
    ByQuantity
}

public enum AuditSecurityLevel
{
    Public,
    Protected,
    Privileged
}

public enum AuditLogStatus
{
    Succeeded,
    Redirected,
    ClientError,
    Blocked,
    ServerError
}
