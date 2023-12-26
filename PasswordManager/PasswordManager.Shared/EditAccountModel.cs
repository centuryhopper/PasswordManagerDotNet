using System.ComponentModel.DataAnnotations;
namespace PasswordManager.Shared;

public class EditAccountModel
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    public string? OldPassword { get; set; }

    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$|^$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
    public string? NewPassword { get; set; }

    [Compare(nameof(NewPassword), ErrorMessage = "The password and confirmation password do not match.")]
    public string? ConfirmNewPassword { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Role { get; set; }

    public override string ToString()
    {
        return $"{nameof(Email)}:{Email}, {nameof(OldPassword)}:{OldPassword}, ";
    }
}
