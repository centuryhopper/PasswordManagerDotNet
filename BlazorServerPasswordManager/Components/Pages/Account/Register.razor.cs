using Microsoft.AspNetCore.Components;

namespace BlazorServerPasswordManager.Pages;

public class RegisterBase : ComponentBase
{
    protected UserAccount Model = new();
    protected bool PasswordVisible = false;
    public bool loading = false;

    [Inject]
    IAccountRepository accountRepository { get; set; }

    
    [Inject]
    public NavigationManager? NavManager { get; set; }

    protected bool ShouldLogin = false;
    protected bool DidRegisterSucceed = false;

    protected async Task OnShouldLoginClose(bool accepted)
    {
        ShouldLogin = false;
    }

    protected override async Task OnInitializedAsync()
    {
    }

    protected async Task OnRegisterComplete(bool accepted)
    {
        if (accepted)
        {
            DidRegisterSucceed = false;
        }

        Model = new();
    }

    protected async Task HandleRegister()
    {
        loading = true;
        var result = await accountRepository.RegisterAsync(Model);
        loading = false;

        if (result.Successful)
        {
            DidRegisterSucceed = true;
        }
        else
        {
            ShowErrors();
        }
    }

    protected void ShowErrors()
    {
        ShouldLogin = true;
    }
}