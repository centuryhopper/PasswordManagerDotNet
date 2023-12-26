using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using BlazorServerPasswordManager.Contexts;
using BlazorServerPasswordManager.Interfaces;

namespace BlazorServerPasswordManager.Components.Pages;

public class PasswordTableBase : ComponentBase
{
    protected IEnumerable<PasswordmanagerAccount> PasswordAccountModels;
    protected RadzenDataGrid<PasswordmanagerAccount> dataGrid;
    protected PasswordmanagerAccount? PasswordToInsert = null;
    protected PasswordmanagerAccount? PasswordToUpdate = null;
    protected Dictionary<string, bool> PasswordVisible = new();

    [Inject]
    IPasswordManagerAccountRepository<PasswordmanagerAccount> passwordManagerAccountRepository { get; set; }
    [Inject]
    AuthenticationStateProvider authStateProvider { get; set; }

    [Inject]
    IJSRuntime jsRuntime { get; set; }

    protected string UserId { get; set; } = "";

    protected void Reset()
    {
        PasswordToInsert = null;
        PasswordToUpdate = null;
    }

    protected override async Task OnInitializedAsync()
    {
        // TODO: get userid from user claim and pass it to an api call for getting the table
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        // UserId = user.FindFirst(c=>c.Type == "sub")?.Value;

        // var user = await accountApiService.GetUserProfileAsync();

        // if (user is null)
        // {
        //     System.Console.WriteLine("User not found :(");
        //     return;
        // }

        // UserId = user.Id;

        PasswordAccountModels = await passwordManagerAccountRepository.GetAllAccountsAsync(UserId!);

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

        await passwordManagerAccountRepository.UpdateAsync(model);
    }

    protected async Task SaveRow(PasswordmanagerAccount model)
    {
        // System.Console.WriteLine(model);
        await dataGrid.UpdateRow(model);

        PasswordAccountModels = await passwordManagerAccountRepository.GetAllAccountsAsync(UserId);

        PasswordToInsert = PasswordToUpdate = null;
    }

    protected async Task OnCreateRow(PasswordmanagerAccount model)
    {
        PasswordToInsert = null;

        // track password visibility
        PasswordVisible[model.Id] = false;

        var create = await passwordManagerAccountRepository.CreateAsync(model);

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

    protected bool DeleteDialogOpen { get; set; } = false;
    protected PasswordmanagerAccount accountToDelete { get; set; }

    protected async Task OnDeleteDialogClose(bool accepted)
    {
        if (accepted)
        {
            await passwordManagerAccountRepository.DeleteAsync(accountToDelete);
            await dataGrid.Reload();
            PasswordAccountModels = await passwordManagerAccountRepository.GetAllAccountsAsync(UserId);
            accountToDelete = null;
        }
        else
        {
            dataGrid.CancelEditRow(accountToDelete);
            await dataGrid.Reload();
        }

        DeleteDialogOpen = false;
        StateHasChanged();
    }

    private void OpenDeleteDialog(PasswordmanagerAccount acc)
    {
        DeleteDialogOpen = true;
        accountToDelete = acc;
        StateHasChanged();
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

        if (PasswordAccountModels.FirstOrDefault(acc => acc.Id == model.Id) is not null)
        {
            // open delete dialog
            OpenDeleteDialog(model);
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
            builder.AppendLine($"{acc.Title},{acc.Username},{acc.Password.Replace(",", "+")},{acc.CreatedAt},{acc.LastUpdatedAt}");
        }

        // await jsRuntime.InvokeVoidAsync("alert", "hello");

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

