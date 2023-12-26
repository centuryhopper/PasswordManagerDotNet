using PasswordManager.Client;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PasswordManager.Shared;
using Blazored.Modal;
using PasswordManager.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

var builder = WebAssemblyHostBuilder.CreateDefault(args);


builder.Services.AddBlazoredModal();

builder.Services.AddScoped(http => new HttpClient {
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});
builder.Services.AddScoped<IPasswordAccountApiService<PasswordmanagerAccount>, PasswordAccountApiService>();

builder.Services
    .AddHttpClient(
        "PasswordManager",
        client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
    )
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

// Supply HttpClient instances that include access tokens when making requests
// to the server project
builder.Services.AddScoped(
    sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("PasswordManager")
);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

await builder.Build().RunAsync();
