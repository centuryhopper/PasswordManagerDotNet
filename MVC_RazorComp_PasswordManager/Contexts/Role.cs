﻿using System;
using System.Collections.Generic;

namespace MVC_RazorComp_PasswordManager.Contexts;

public partial class Role
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<Userrole> Userroles { get; set; } = new List<Userrole>();
}
