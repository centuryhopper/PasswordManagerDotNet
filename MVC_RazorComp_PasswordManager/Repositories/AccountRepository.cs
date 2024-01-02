using System.Collections;
using MVC_RazorComp_PasswordManager.Contexts;
using Microsoft.EntityFrameworkCore;
using MVC_RazorComp_PasswordManager.Models;
using MVC_RazorComp_PasswordManager.Interfaces;

namespace MVC_RazorComp_PasswordManager.Repositories;

/*
insert into roles
values ('056k16c9-07fb-4184-b1e6-89df8474690f','Admin');
insert into roles
values ('930013u9-07fb-4184-b1e6-072f4474690f','User');

INSERT INTO userroles
values('056c56c9-07fb-4184-b1e6-89df8474690f','056k16c9-07fb-4184-b1e6-89df8474690f')
*/

public class AccountRepository : IAccountRepository
{
    private readonly EncryptionContext encryptionContext;
    private readonly PasswordAccountContext passwordAccountContext;

    public AccountRepository(EncryptionContext encryptionContext, PasswordAccountContext passwordAccountContext)
    {
        this.encryptionContext = encryptionContext;
        this.passwordAccountContext = passwordAccountContext;
    }
    public async Task<DeleteUserProfileStatus> DeleteUserAsync(string Id)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<string>> GetRolesAsync()
    {
        return from role in await passwordAccountContext.Roles.ToListAsync() select role.Name;
    }

    public async Task<PasswordmanagerUser?> GetUserByIdAsync(string userId)
    {
        var user = await passwordAccountContext.PasswordmanagerUsers.FindAsync(userId);
        return user;
    }

    public async Task<EmailConfirmStatus> IsEmailConfirmed(string email)
    {
        var user = await passwordAccountContext.PasswordmanagerUsers.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null)
        {
            return EmailConfirmStatus.ACCOUNT_NOT_REGISTERED;
        }
        return user.Emailconfirmed.Get(0) ? EmailConfirmStatus.CONFIRMED : EmailConfirmStatus.NOT_CONFIRMED;
    }

    private UserModel? GetUserByEmailAsync(string email)
    {
        var users = passwordAccountContext.PasswordmanagerUsers.AsQueryable();
        var roles = passwordAccountContext.Roles.AsQueryable();
        var userroles = passwordAccountContext.Userroles.AsQueryable();

        var dbResult = from u in users
                       where u.Email == email
                       join ur in passwordAccountContext.Userroles on u.Id equals ur.Userid into userRoles_g
                       from userRole in userRoles_g.DefaultIfEmpty()
                       join r in passwordAccountContext.Roles on userRole.Roleid equals r.Id into roles_g
                       from r in roles_g.DefaultIfEmpty()
                       select new UserModel
                       {
                           Id = u.Id,
                           Salt = u.Salt,
                           PasswordHash = u.Passwordhash,
                           Email = u.Email,
                           FirstName = u.Firstname,
                           LastName = u.Lastname,
                           Role = r.Name
                       };

        return dbResult.FirstOrDefault();
    }

    public async Task<AuthStatus> LoginAsync(LoginModel model)
    {
        // get user from db
        // if user in db matches user logging in
        // then return the user
        // otherwise return null

        var userModel = GetUserByEmailAsync(model.Email);

        // PasswordmanagerUser dbResult = null;

        if (userModel is null)
        {
            return new AuthStatus { Error = "User Not Found", Successful = false };
        }

        var hashedPW = encryptionContext.OneWayHash($"{model.Password}{userModel.Salt}");

        if (hashedPW == userModel.PasswordHash)
        {
            // update login userfield
            var user = await passwordAccountContext.PasswordmanagerUsers.FindAsync(userModel.Id);
            user!.Datelastlogin = DateTime.Now;
            await passwordAccountContext.SaveChangesAsync();

            return new AuthStatus { Id = userModel.Id, Name = userModel.FirstName + " " + userModel.LastName, Role = userModel.Role, Email = userModel.Email, Successful = true };
        }

        return new AuthStatus { Error = "Invalid username or password", Successful = false };
    }

    public async Task<AuthStatus> LogoutAsync(string userId)
    {
        // update logout userfield
        var user = await passwordAccountContext.PasswordmanagerUsers.FindAsync(userId);
        user!.Datelastlogout = DateTime.Now;
        await passwordAccountContext.SaveChangesAsync();
        return new AuthStatus { Successful = true };
    }

    public async Task<AuthStatus> RegisterAsync(RegisterModel model)
    {
        var dbResult = GetUserByEmailAsync(model.Email);

        if (dbResult is not null)
        {
            return new AuthStatus { Error = "Can't register because user already exists", Successful = false };
        }

        try
        {
            var Id = Guid.NewGuid().ToString();
            int salt = new Random().Next();
            var saltedPW = $"{model.Password}{salt}";
            var passwordHash = encryptionContext.OneWayHash(saltedPW);

            await passwordAccountContext.PasswordmanagerUsers.AddAsync(
                new PasswordmanagerUser
                {
                    Id = Id,
                    Email = model.Email,
                    Salt = salt.ToString(),
                    Passwordhash = passwordHash,
                    Firstname = model.FirstName,
                    Lastname = model.LastName,
                    // TODO: mark as false when done testing registration
                    Emailconfirmed = new BitArray(new bool[] { true }),
                    Lockoutenabled = new BitArray(new bool[] { false }),
                    Lockoutenddateutc = null,
                    Accessfailedcount = 0,
                    Datelastlogin = DateTime.Now,
                    Datelastlogout = null,
                }
            );


            // assign role of "User" to this user
            var role = await passwordAccountContext.Roles.FirstAsync(r => r.Name == "User");
            await passwordAccountContext.Userroles.AddAsync(new Userrole { Roleid = role.Id, Userid = Id });
            await passwordAccountContext.SaveChangesAsync();

            return new AuthStatus { Successful = true, Id = Id, Email = model.Email, Name = model.FirstName + " " + model.LastName, Role = role.Name };
        }
        catch (System.Exception e)
        {
            return new AuthStatus { Successful = false, Error = e.Message };
        }
    }

    public async Task<IEnumerable<string>> UpdateUserAsync(EditAccountModel model)
    {
        // yell at the user if old password is incorrect
        var userModel = await GetUserByIdAsync(model.Id!);

        if (userModel is null)
        {
            return ["User Not Found"];
        }

        var hashedPW = encryptionContext.OneWayHash($"{model.OldPassword}{userModel.Salt}");

        if (hashedPW != userModel.Passwordhash)
        {
            return ["Your old password is incorrect"];
        }

        // modify fields
        userModel.Firstname = model.FirstName;
        userModel.Lastname = model.LastName;

        int salt = new Random().Next();
        var saltedPW = $"{model.NewPassword}{salt}";
        var passwordHash = encryptionContext.OneWayHash(saltedPW);
        userModel.Salt = salt.ToString();
        userModel.Passwordhash = passwordHash;

        // TODO: change role as well if different
        // delete old userrole link and add new one

        // TODO: send confirmation email if user entered a new email
        // only remove the current working email once confirmed
        // in case they make a mistake and typed in the wrong email and cant log back in

        await passwordAccountContext.SaveChangesAsync();

        return ["success!"];
    }

    public async Task<bool> VerifyToken(AccountProviders accountProviders, string token, string userId)
    {
        throw new NotImplementedException();
    }
}