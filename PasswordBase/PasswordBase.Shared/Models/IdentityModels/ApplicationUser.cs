using Microsoft.AspNetCore.Identity;

namespace PasswordBase.Shared.Models.IdentityModels;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    // public List<AccountModel>? accounts { get; set; }
}
