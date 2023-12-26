using BlazorServerPasswordManager.Contexts;
using CsvHelper.Configuration;

namespace BlazorServerPasswordManager;

public class PasswordsMapper : ClassMap<PasswordmanagerAccount>
{
    public PasswordsMapper()
    {
        Map(m => m.Title).Name("Title");
        Map(m => m.Username).Name("Username");
        Map(m => m.Password).Name("Password");
    }
}