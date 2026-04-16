using Microsoft.AspNetCore.Identity;

namespace LamillaEscudero.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public string? Cedula { get; set; }
    public bool IsActive { get; set; } = true;
}