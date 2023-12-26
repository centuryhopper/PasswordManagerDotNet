using System.ComponentModel.DataAnnotations;
namespace PasswordManager.Shared;

public class AccountModel
{
    [Required(ErrorMessage = "Please enter a valid email address."), EmailAddress]
    public string Email { get; set; }
    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public override string ToString()
    {
        return $"{nameof(Email)}:{Email}, {nameof(Password)}:{Password}, ";
    }
}
