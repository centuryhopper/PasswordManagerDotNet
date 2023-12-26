using BlazorServerPasswordManager;
using BlazorServerPasswordManager.Authentication;
using BlazorServerPasswordManager.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorServerPasswordManager.Pages;


public class LoginBase : ComponentBase
{
    protected LoginModel Model = new();
    protected bool PasswordVisible = false;
    protected bool loading = false;
    protected bool ShouldRegister = false;
    protected bool ShouldConfirmEmail = false;
    protected bool InvalidLogin = false;

    [Inject]
    IAccountRepository accountRepository { get; set; }

    [Inject]
    NavigationManager? NavManager { get; set; }

    [Inject] AuthenticationStateProvider authStateProvider { get; set; }

    protected override void OnInitialized()
    {
        // base.OnInitialized();
    }

    
    protected async Task OnShouldConfirmEmailClose(bool accepted)
    {
        if (accepted)
        {
            // TODO: send out email
        }
        ShouldConfirmEmail = false;
    }

    protected async Task OnShouldRegisterClose(bool accepted)
    {
        if (accepted)
        {
            NavManager.NavigateTo("register", forceLoad: true);
        }
        ShouldRegister = false;
    }

    protected async Task OnInvalidLogin(bool accepted)
    {
        if (accepted)
        {
            NavManager.NavigateTo("register", forceLoad: true);
        }
        InvalidLogin = false;
    }

    protected async Task HandleLogin()
    {
        loading = true;
        // if email isn't confirmed then send error pop up saying that you have to confirm email first
        if (await accountRepository.IsEmailConfirmed(Model.Email) == EmailConfirmStatus.NOT_CONFIRMED)
        {
            // Modal.Show<AuthenticationErrorModal>("Error", new ModalParameters { { "ErrorMessage", "Please confirm your email before logging in." } });
            loading = false;
            return;
        }

        // we don't tell the user that this email isn't in our database to avoid data breaching
        // that's why we give a generic error message below
        if (await accountRepository.IsEmailConfirmed(Model.Email) == EmailConfirmStatus.ACCOUNT_NOT_REGISTERED)
        {
            ShouldRegister = true;
            loading = false;
            return;
        }

        var result = await accountRepository.LoginAsync(Model);

        loading = false;

        if (result.Successful)
        {
            var customAuthStateProvider = (CustomAuthenticationStateProvider)authStateProvider;
            await customAuthStateProvider.UpdateAuthenticationState(new UserSession
            {
                UserName = result.Email!,
                Role = result.Role!,
            });
            NavManager.NavigateTo("passwords");
        }
        else
        {
            ShowErrors();
        }
    }

    protected void ShowErrors()
    {
        InvalidLogin = true;
    }
}