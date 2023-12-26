using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using PasswordManager.Client.Pages.Shared;

namespace PasswordManager.Client.Pages;


public class SettingsBase : ComponentBase
{
    [CascadingParameter]
    protected IModalService Modal { get; set; }

    [Inject]
    NavigationManager NavManager { get; set; }


    protected override void OnInitialized()
    {
        base.OnInitialized();
    }

    protected void HandleEdit()
    {
        Modal.Show<EditProfileModal>();
    }

    protected async Task HandleDeleteUserProfile()
    {
        var confirmModal = Modal.Show<ConfirmationModal>("Warning!");
        var modalResult = await confirmModal.Result;

        // If the modal was not cancelled, take the action
        if (modalResult.Confirmed)
        {
            Modal.Show<PasswordModal>("Enter your email and password");
        }
    }


}