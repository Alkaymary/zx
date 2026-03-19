namespace MyApi.Infrastructure.Secrets;

public static class SecretNames
{
    public const string DefaultConnectionString = "ConnectionStrings:DefaultConnection";
    public const string JwtSecretKey = "Jwt:SecretKey";
    public const string BootstrapAdminPassword = "BootstrapAdmin:Password";
}
