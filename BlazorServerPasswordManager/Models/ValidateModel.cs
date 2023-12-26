using System.ComponentModel.DataAnnotations;
namespace BlazorServerPasswordManager;

public class ValidateModel
{
    [Required, EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }

    public override string ToString()
    {
        return $"{nameof(Email)}:{Email}, {nameof(Password)}:{Password}, ";
    }
}
