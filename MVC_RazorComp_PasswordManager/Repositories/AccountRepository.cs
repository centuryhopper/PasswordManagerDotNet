using System.Collections;
using MVC_RazorComp_PasswordManager.Contexts;
using Microsoft.EntityFrameworkCore;
using MVC_RazorComp_PasswordManager.Models;
using MVC_RazorComp_PasswordManager.Interfaces;
using System.Net.Mail;
using System.Net;

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
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IConfiguration config;

    public AccountRepository(EncryptionContext encryptionContext, PasswordAccountContext passwordAccountContext, IHttpContextAccessor httpContextAccessor, IConfiguration config)
    {
        this.encryptionContext = encryptionContext;
        this.passwordAccountContext = passwordAccountContext;
        this.httpContextAccessor = httpContextAccessor;
        this.config = config;
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
        // var userroles = passwordAccountContext.Userroles.AsQueryable();
        List<string> userroles = [ ];

        List<UserModel> dbResult = [];

        // var dbResult = from u in users
        //                where u.Email == email
        //                join ur in passwordAccountContext.Userroles on u.Id equals ur.Userid into userRoles_g
        //                from userRole in userRoles_g.DefaultIfEmpty()
        //                join r in passwordAccountContext.Roles on userRole.Roleid equals r.Id into roles_g
        //                from r in roles_g.DefaultIfEmpty()
        //                select new UserModel
        //                {
        //                    Id = u.Id,
        //                    Salt = u.Salt,
        //                    PasswordHash = u.Passwordhash,
        //                    Email = u.Email,
        //                    FirstName = u.Firstname,
        //                    LastName = u.Lastname,
        //                    Role = r.Name
        //                };

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
            // await passwordAccountContext.Userroles.AddAsync(new Userrole { Roleid = role.Id, Userid = Id });
            await passwordAccountContext.SaveChangesAsync();

            // create and send email confirmation link
            var token = TokenGenerator.GenerateToken(32);

            var TokenPK = Guid.NewGuid().ToString();
            var LoginProvider = AccountProviders.EMAIL_CONFIRMATION.ToString();
            // make sure there are no spaces to preserve consistent token identity when passing thru urls
            var ProviderKey = token.Replace(" ", "+");
            var UserIdFK = Id;

            await passwordAccountContext.Usertokens.AddAsync(new Usertoken
            {
                Id = TokenPK,
                Loginprovider = LoginProvider,
                Providerkey = ProviderKey,
                Userid = UserIdFK,
            });

            // store token in user token table
            SendConfirmationEmail(model.Email, $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}/confirm-email/?token={token}&userId={UserIdFK}");

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
        var loginProvider = accountProviders.ToString();
        try
        {
            var result = await passwordAccountContext.Usertokens.FirstAsync(ut => ut.Userid == userId && ut.Loginprovider == loginProvider);

            // we know the any spaces of the token stored in the db has pluses replaced them, so we do this with our current token as well
            token = token.Replace(" ", "+");

            if (string.IsNullOrEmpty(result.Providerkey) || result.Providerkey != token)
            {
                Console.WriteLine($"token: {token}");
                Console.WriteLine($"result: {result}");
                return false;
            }

            // mark email as confirmed
            var updatedUserEmailConfirmed = await passwordAccountContext.PasswordmanagerUsers.FirstAsync(u => u.Id == userId);
            updatedUserEmailConfirmed.Emailconfirmed.Set(0, true);

            await passwordAccountContext.SaveChangesAsync();

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }

    private void SendConfirmationEmail(string recipientEmail, string confirmationLink)
    {
        // Configure email settings
        string senderEmail = config.GetSection("EmailSettings:SenderEmail").Value!;
        string senderPassword = config.GetSection("EmailSettings:SenderPassword").Value!;

        // Console.WriteLine(senderEmail);
        // Console.WriteLine(senderPassword);

        string smtpServer = "smtp.gmail.com";
        int smtpPort = 587;

        // Create the email message
        MailMessage message = new();
        message.From = new MailAddress(senderEmail);
        message.To.Add(recipientEmail);
        message.Subject = "Email Confirmation";
        message.Body = $"Please confirm your email by clicking the following link: {confirmationLink}";
        message.IsBodyHtml = false;

        // Configure SMTP client
        SmtpClient smtpClient = new(smtpServer, smtpPort);
        smtpClient.EnableSsl = true;
        smtpClient.UseDefaultCredentials = false;
        smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);

        // Send the email
        try
        {
            smtpClient.Send(message);
        }
        catch (SmtpException ex)
        {
            // Handle any exceptions or log errors
            Console.WriteLine($"Failed to send confirmation email: {ex.Message}");
        }
    }

}