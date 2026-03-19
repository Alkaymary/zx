using Microsoft.Extensions.DependencyInjection;
using MyApi.Application.AdminUsers;
using MyApi.Application.AuditLogs;
using MyApi.Application.Auth;
using MyApi.Application.FinancialTransactions;
using MyApi.Application.Libraries;
using MyApi.Application.LibraryAccounts;
using MyApi.Application.LibraryFinancial;
using MyApi.Application.Packages;
using MyApi.Application.PosDevices;
using MyApi.Application.QrCodes;
using MyApi.Application.Roles;
using MyApi.Application.WeatherForecasting;

namespace MyApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ListFinancialTransactionsQuery>();
        services.AddScoped<GetFinancialTransactionByIdQuery>();
        services.AddScoped<GetTransactionSettlementByIdQuery>();
        services.AddScoped<GetLibraryFinancialStatementQuery>();
        services.AddScoped<CreateFinancialTransactionUseCase>();
        services.AddScoped<UpdateFinancialTransactionUseCase>();
        services.AddScoped<DeleteFinancialTransactionUseCase>();
        services.AddScoped<CreateTransactionSettlementUseCase>();
        services.AddScoped<CreateLibrarySettlementUseCase>();
        services.AddScoped<DeleteTransactionSettlementUseCase>();

        services.AddScoped<ListLibrariesQuery>();
        services.AddScoped<SearchLibrariesQuery>();
        services.AddScoped<GetLibraryStatsQuery>();
        services.AddScoped<GetLibraryByIdQuery>();
        services.AddScoped<CreateLibraryUseCase>();
        services.AddScoped<UpdateLibraryUseCase>();
        services.AddScoped<UpdateLibraryByQueryUseCase>();
        services.AddScoped<DeleteLibraryUseCase>();
        services.AddScoped<DeleteLibraryByQueryUseCase>();

        services.AddScoped<ListLibraryAccountsQuery>();
        services.AddScoped<GetLibraryAccountByIdQuery>();
        services.AddScoped<GetAvailableLibraryAccountRolesQuery>();
        services.AddScoped<ValidateLibraryAccountDependencies>();
        services.AddScoped<CreateLibraryAccountUseCase>();
        services.AddScoped<UpdateLibraryAccountUseCase>();
        services.AddScoped<UpdateLibraryAccountByQueryUseCase>();
        services.AddScoped<DeleteLibraryAccountUseCase>();
        services.AddScoped<DeleteLibraryAccountByQueryUseCase>();

        services.AddScoped<ListAdminUsersQuery>();
        services.AddScoped<GetAdminUserByIdQuery>();
        services.AddScoped<ValidateAdminUserDependencies>();
        services.AddScoped<CreateAdminUserUseCase>();
        services.AddScoped<UpdateAdminUserUseCase>();
        services.AddScoped<UpdateAdminUserByQueryUseCase>();
        services.AddScoped<DeleteAdminUserUseCase>();
        services.AddScoped<DeleteAdminUserByQueryUseCase>();

        services.AddScoped<ListPackagesQuery>();
        services.AddScoped<GetPackageByIdQuery>();
        services.AddScoped<ValidatePackageDependencies>();
        services.AddScoped<CreatePackageUseCase>();
        services.AddScoped<UpdatePackageUseCase>();
        services.AddScoped<DeletePackageUseCase>();

        services.AddScoped<ListPosDevicesQuery>();
        services.AddScoped<GetPosDeviceByIdQuery>();
        services.AddScoped<ValidatePosDeviceDependencies>();
        services.AddScoped<CreatePosDeviceUseCase>();
        services.AddScoped<UpdatePosDeviceUseCase>();
        services.AddScoped<UpdatePosDeviceByQueryUseCase>();
        services.AddScoped<DeletePosDeviceUseCase>();
        services.AddScoped<DeletePosDeviceByQueryUseCase>();

        services.AddScoped<GetLibraryQrActorDataQuery>();
        services.AddScoped<ListLibraryQrCodesQuery>();
        services.AddScoped<GetLibraryQrCodeByIdQuery>();
        services.AddScoped<GetAdminQrLibraryMetricsQuery>();
        services.AddScoped<ListAdminLibraryQrItemsQuery>();
        services.AddScoped<GetAdminQrByReferenceQuery>();
        services.AddScoped<ExportLibraryQrCodeUseCase>();
        services.AddScoped<UpdateLibraryQrCodeUseCase>();
        services.AddScoped<DeleteLibraryQrCodeUseCase>();

        services.AddScoped<IAdminAuthAppService, AdminAuthAppService>();
        services.AddScoped<ILibraryAuthAppService, LibraryAuthAppService>();
        services.AddScoped<IRoleAppService, RoleAppService>();
        services.AddScoped<IAdminUsersAppService, AdminUsersAppService>();
        services.AddScoped<IAuditLogsAppService, AuditLogsAppService>();
        services.AddScoped<IPackagesAppService, PackagesAppService>();
        services.AddScoped<ILibrariesAppService, LibrariesAppService>();
        services.AddScoped<ILibraryAccountsAppService, LibraryAccountsAppService>();
        services.AddScoped<IFinancialTransactionsAppService, FinancialTransactionsAppService>();
        services.AddScoped<ILibraryFinancialAppService, LibraryFinancialAppService>();
        services.AddScoped<IPosDevicesAppService, PosDevicesAppService>();
        services.AddScoped<ILibraryQrCodesAppService, LibraryQrCodesAppService>();
        services.AddScoped<IAdminQrCodesAppService, AdminQrCodesAppService>();
        services.AddScoped<IWeatherForecastAppService, WeatherForecastAppService>();

        return services;
    }
}
