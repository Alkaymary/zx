using System.ComponentModel.DataAnnotations;
using MyApi.Models;

namespace MyApi.Dtos;

public class RoleResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public GuardName GuardName { get; set; }
    public DateTime CreatedAt { get; set; }
    public int LibraryAccountsCount { get; set; }
}

public class CreateRoleDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Code { get; set; } = string.Empty;

    [Required]
    public GuardName GuardName { get; set; }
}

public class UpdateRoleDto : CreateRoleDto
{
}
