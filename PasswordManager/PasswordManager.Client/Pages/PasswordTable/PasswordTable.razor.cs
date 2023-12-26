using System.Text;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PasswordManager.Client.Pages.Shared;
using PasswordManager.Shared;
using Radzen;
using Radzen.Blazor;

namespace PasswordManager.Client.Pages;

public class PasswordTableBase : ComponentBase
{
    protected IEnumerable<PasswordmanagerAccount> PasswordAccountModels;
    protected RadzenDataGrid<PasswordmanagerAccount?>? dataGrid;
    protected PasswordmanagerAccount? PasswordToInsert = null;
    protected PasswordmanagerAccount? PasswordToUpdate = null;
    protected Dictionary<string, bool> PasswordVisible = new();

    [CascadingParameter] IModalService? Modal { get; set; }

    [Inject]
    IPasswordAccountApiService<PasswordmanagerAccount> passwordAccountApiService { get; set; }
    // [Inject]
    // AuthenticationStateProvider authStateProvider {get;set;}

    [Inject]
    IJSRuntime jsRuntime {get;set;}

    protected string UserId { get; set; } = "";

    protected void Reset()
    {
        PasswordToInsert = null;
        PasswordToUpdate = null;
    }

    protected override async Task OnInitializedAsync()
    {
        // TODO: get userid from user claim and pass it to an api call for getting the table
        // var authState = await authStateProvider.GetAuthenticationStateAsync();
        // var user = authState.User;

        // UserId = user.FindFirst(c=>c.Type == "sub")?.Value;

        // var user = await accountApiService.GetUserProfileAsync();

        // if (user is null)
        // {
        //     System.Console.WriteLine("User not found :(");
        //     return;
        // }

        // UserId = user.Id;

        PasswordAccountModels = await passwordAccountApiService.GetPasswordAccounts(UserId!);

        System.Console.WriteLine(UserId);

        Console.WriteLine(PasswordAccountModels.Count());

        // foreach (var account in PasswordAccountModels)
        // {
        //     System.Console.WriteLine(account.CreatedAt);
        // }

    }

    protected bool GetPasswordVisibility(string Id)
    {
        if (!PasswordVisible.ContainsKey(Id))
        {
            PasswordVisible.Add(Id, false);
        }

        return PasswordVisible[Id];
    }

    protected async Task EditRow(PasswordmanagerAccount model)
    {
        PasswordToUpdate = model;

        // changes the row into edit mode (deactivate template and activate edittemplate)
        await dataGrid.EditRow(model);
    }

    protected async Task OnUpdateRow(PasswordmanagerAccount model)
    {
        // reset insert variable
        if (PasswordToInsert == model)
        {
            PasswordToInsert = null;
        }

        PasswordToUpdate = null;

        await passwordAccountApiService.UpdateAsync(model);
    }

    protected async Task SaveRow(PasswordmanagerAccount model)
    {
        // System.Console.WriteLine(model);
        await dataGrid.UpdateRow(model);

        PasswordAccountModels = await passwordAccountApiService.GetPasswordAccounts(UserId);

        PasswordToInsert = PasswordToUpdate = null;
    }

    protected async Task OnCreateRow(PasswordmanagerAccount model)
    {
        PasswordToInsert = null;

        // track password visibility
        PasswordVisible[model.Id] = false;

        var create = await passwordAccountApiService.CreateAsync(model);

        // System.Console.WriteLine(create);
    }

    protected Task CancelEdit(PasswordmanagerAccount model)
    {
        if (model == PasswordToInsert)
        {
            PasswordToInsert = null;
        }

        PasswordVisible.Remove(model.Id);

        PasswordToUpdate = null;

        dataGrid.CancelEditRow(model);
        return Task.CompletedTask;
    }

    protected async Task DeleteRow(PasswordmanagerAccount model)
    {
        if (model == PasswordToInsert)
        {
            PasswordToInsert = null;
        }

        if (model == PasswordToUpdate)
        {
            PasswordToUpdate = null;
        }

        if (PasswordAccountModels.Contains(model))
        {
            var confirmModal = Modal.Show<ConfirmationModal>("Warning");
            var modalResult = await confirmModal.Result;

            if (modalResult.Confirmed)//if the modal was not cancelled, take the action 
            {
                await passwordAccountApiService.DeleteAsync(model);
                await dataGrid.Reload();
                PasswordAccountModels = await passwordAccountApiService.GetPasswordAccounts(UserId);

            }

        }
        else
        {
            dataGrid.CancelEditRow(model);
            await dataGrid.Reload();
        }
    }

    protected async Task InsertRow()
    {
        PasswordToInsert = new PasswordmanagerAccount
        {
            Id = Guid.NewGuid().ToString(),
            Userid = UserId!,
        };

        await dataGrid!.InsertRow(PasswordToInsert);
    }

    protected async void Export()
    {
        StringBuilder builder = new();

        builder.AppendLine("Title,Username,Password,CreatedAt,LastUpdatedAt");

        foreach (var acc in PasswordAccountModels)
        {
            builder.AppendLine($"{acc.Title},{acc.Username},{acc.Password.Replace(",","+")},{acc.CreatedAt},{acc.LastUpdatedAt}");
        }

        await jsRuntime.InvokeVoidAsync("saveAsFile", "sensitive.csv", builder.ToString());
    }

    protected RadzenUpload upload;

    protected void OnChange(UploadChangeEventArgs args, string name)
    {
        foreach (var file in args.Files)
        {
            Console.WriteLine($"File: {file.Name} / {file.Size} bytes");
        }

        Console.WriteLine($"{name} changed");
    }
    protected void OnProgress(UploadProgressArgs args, string name)
    {
        Console.WriteLine($"{args.Progress}% '{name}' / {args.Loaded} of {args.Total} bytes.");

        if (args.Progress == 100)
        {
            foreach (var file in args.Files)
            {
                Console.WriteLine($"Uploaded: {file.Name} / {file.Size} bytes");
            }
        }
    }
}

