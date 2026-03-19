namespace MyApi.Dtos;

public class AdminLoginRequestDto
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public class LibraryLoginRequestDto
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public string TokenType { get; set; } = "Bearer";

    public DateTime ExpiresAt { get; set; }

    public int UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string RoleCode { get; set; } = string.Empty;
}

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class LibraryAuthResponseDto : AuthResponseDto
{
    public LibraryAccountProfileDto Account { get; set; } = new();

    public LibraryProfileDto Library { get; set; } = new();

    public List<LibraryPosDeviceDto> PosDevices { get; set; } = new();
}

public class LibraryMeResponseDto
{
    public LibraryAccountProfileDto Account { get; set; } = new();

    public LibraryProfileDto Library { get; set; } = new();

    public List<LibraryPosDeviceDto> PosDevices { get; set; } = new();
}

public class LibraryAccountProfileDto
{
    public int UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string RoleCode { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime? LastLoginAt { get; set; }
}

public class LibraryProfileDto
{
    public int LibraryId { get; set; }

    public string LibraryCode { get; set; } = string.Empty;

    public string LibraryName { get; set; } = string.Empty;

    public string? OwnerName { get; set; }

    public string? OwnerPhone { get; set; }

    public string? City { get; set; }

    public string Status { get; set; } = string.Empty;
}

public class LibraryPosDeviceDto
{
    public int Id { get; set; }

    public string PosCode { get; set; } = string.Empty;

    public string? SerialNumber { get; set; }

    public string? DeviceModel { get; set; }

    public string? DeviceVendor { get; set; }

    public string Status { get; set; } = string.Empty;

    public bool IsActivated { get; set; }

    public DateTime? ActivatedAt { get; set; }

    public DateTime? LastAuthenticatedAt { get; set; }
}
