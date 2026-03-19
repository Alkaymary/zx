namespace MyApi.Application.Common.Contexts;

public sealed record AdminActorContext(int? UserId, bool IsAdminUser, bool CanViewLibraryFinancialData);
