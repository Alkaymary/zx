using System.ComponentModel.DataAnnotations;

namespace MyApi.Models;

public class Role
{
    public int Id { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Code { get; set; } = string.Empty;

    public GuardName GuardName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AdminUser> AdminUsers { get; set; } = new List<AdminUser>();

    public ICollection<LibraryAccount> LibraryAccounts { get; set; } = new List<LibraryAccount>();
}
