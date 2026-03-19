namespace MyApi.Options;

public class BootstrapAdminOptions
{
    public const string SectionName = "BootstrapAdmin";

    public bool Enabled { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string Password { get; set; } = string.Empty;

    public string RoleCode { get; set; } = "super_admin";
}
