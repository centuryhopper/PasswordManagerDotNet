using System.Collections;
using BlazorServerPasswordManager;
using BlazorServerPasswordManager.Contexts;
using Microsoft.EntityFrameworkCore;

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

    public async Task<IEnumerable<string?>?> GetRolesAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<UserModel?> GetUserByIdAsync(string UserId)
    {
        throw new NotImplementedException();
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

    private async Task<UserModel?> GetUserByEmailAsync(string email)
    {
        var dbResult = await passwordAccountContext
        .Userroles
        .Include(ur => ur.Role)
        .Include(ur => ur.User)
        .Where(u => u.User.Email.Equals(email))
        .Select(ur => new UserModel{
            Id = ur.Userid,
            Salt = ur.User.Salt,
            PasswordHash = ur.User.Passwordhash,
            Email = ur.User.Email,
            FirstName = ur.User.Firstname,
            LastName = ur.User.Lastname,
            Role = ur.Role.Name,
        }).FirstOrDefaultAsync();

        return dbResult;
    }

    public async Task<AuthStatus> LoginAsync(LoginModel model)
    {
        // get user from db
        // if user in db matches user logging in
        // then return the user
        // otherwise return null

        var userModel = await GetUserByEmailAsync(model.Email);

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
            user!.Datelastlogin = DateTime.UtcNow;
            await passwordAccountContext.SaveChangesAsync();

            return new AuthStatus { Id = userModel.Id, Name = userModel.FirstName + " " + userModel.LastName, Role = userModel.Role, Email = userModel.Email, Successful = true };
        }

        return new AuthStatus { Error = "Invalid username or password", Successful = false };
    }

    public async Task<AuthStatus> LogoutAsync(string userId)
    {
        // update logout userfield
        var user = await passwordAccountContext.PasswordmanagerUsers.FindAsync(userId);
        user!.Datelastlogout = DateTime.UtcNow;
        await passwordAccountContext.SaveChangesAsync();
        return new AuthStatus { Successful = true };
    }

    public async Task<AuthStatus> RegisterAsync(UserAccount model)
    {
        var dbResult = await GetUserByEmailAsync(model.Email);

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
                new PasswordmanagerUser {
                    Id = Id,
                    Email = model.Email,
                    Salt = salt.ToString(),
                    Passwordhash = passwordHash,
                    Firstname = model.FirstName,
                    Lastname = model.LastName,
                    Emailconfirmed = new BitArray(new bool[]{false}),
                    Lockoutenabled = new BitArray(new bool[]{false}),
                    Lockoutenddateutc = null,
                    Accessfailedcount = 0,
                    Datelastlogin = DateTime.Now,
                    Datelastlogout = null,
                }
            );
    
            await passwordAccountContext.SaveChangesAsync();

            return new AuthStatus { Successful = true, Id = Id, Email = model.Email, Name = model.FirstName + " " + model.LastName, Role = "Admin" };
        }
        catch (System.Exception e)
        {
            return new AuthStatus { Successful = false, Error = e.Message };
        }
    }

    public async Task<IEnumerable<string>> UpdateUserAsync(EditAccountModel model)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> VerifyToken(AccountProviders accountProviders, string token, string userId)
    {
        throw new NotImplementedException();
    }
}