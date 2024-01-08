using System;
using System.Collections.Generic;

namespace MVC_RazorComp_PasswordManager.Contexts;

public partial class ContactDb
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string Message { get; set; } = null!;
}
