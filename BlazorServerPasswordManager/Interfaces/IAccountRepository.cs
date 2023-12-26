using BlazorServerPasswordManager;

public interface IAccountRepository
{
    Task<AuthStatus> LoginAsync(LoginModel model);
    Task<AuthStatus> LogoutAsync(string userId);
    Task<AuthStatus> RegisterAsync(UserAccount model);
    Task<UserModel?> GetUserByIdAsync(string UserId);
    Task<IEnumerable<string?>?> GetRolesAsync();
    Task<IEnumerable<string>> UpdateUserAsync(EditAccountModel model);
    Task<DeleteUserProfileStatus> DeleteUserAsync(string Id);
    Task<bool> VerifyToken(AccountProviders accountProviders, string token, string userId);
    Task<EmailConfirmStatus> IsEmailConfirmed(string email);
}
