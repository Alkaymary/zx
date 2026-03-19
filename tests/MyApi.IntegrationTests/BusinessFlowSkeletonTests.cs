namespace MyApi.IntegrationTests;

public sealed class BusinessFlowSkeletonTests
{
    [Fact(Skip = "Skeleton only: seed packages, POS devices, and financial data for an end-to-end office QR export flow.")]
    public Task Office_account_can_export_qr_and_see_financial_charge()
    {
        return Task.CompletedTask;
    }

    [Fact(Skip = "Skeleton only: seed library dues and verify finance can settle while admin receives 403 on write.")]
    public Task Finance_can_create_and_settle_library_dues()
    {
        return Task.CompletedTask;
    }

    [Fact(Skip = "Skeleton only: seed a super admin token and verify admin user creation and role assignment flow.")]
    public Task Super_admin_can_create_admin_user_and_assign_role()
    {
        return Task.CompletedTask;
    }
}
