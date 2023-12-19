using MVC_RazorComp_PasswordManager.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace MVC_RazorComp_PasswordManager.Utilities;

public static class Helpers
{
    public static IEnumerable<string> GetErrors<T>(this ModelStateDictionary ModelState, ILogger<T>? logger = null)
    {
        // Retrieve the list of errors
        var errors = ModelState.Values.SelectMany(v => v.Errors);
        if (logger is not null)
        {
            errors.ToList().ForEach(e => logger.LogWarning(e.ErrorMessage));
        }

        return errors.Select(e => e.ErrorMessage);
    }

    // public static async Task SendEmail<T>(IEmailSender emailSender, string email, string subject, string htmlMessage, ILogger<T> logger)
    // {
    //     try
    //     {
    //         await emailSender.SendEmailAsync(email, subject, htmlMessage);
    //     }
    //     catch (System.Exception ex)
    //     {
    //         logger.LogError(ex.Message);
    //     }
    // }

}