using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Client.Pages;
using PasswordManager.Client.Services;
using PasswordManager.Components;
using PasswordManager.Components.Account;
using PasswordManager.Contexts;
using PasswordManager.Data;
using PasswordManager.Repositories;
using PasswordManager.Shared;

/*
This app uses asp.net identity framework with a database first approach

scaffolding comes first and then comes migrations for identity

dotnet ef dbcontext scaffold "insert_connection_string" Npgsql.EntityFrameworkCore.PostgreSQL -o Data/ -t passwordmanager_accounts

drop table "AspNetUserClaims";
drop table "AspNetRoleClaims";
drop table "AspNetUserRoles";
drop table "AspNetUserTokens";
drop table "AspNetUserLogins";
drop table "AspNetUsers";
drop table "AspNetRoles";
drop table "__EFMigrationsHistory";


dotnet ef migrations add Initial -c ApplicationDbContext
dotnet ef database update -c ApplicationDbContext
*/


// This project was created via this command: dotnet new blazor -o BlazorApp -int Auto -au Individual

var builder = WebApplication.CreateBuilder(args);


// additional services
builder.Services.AddControllers();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("elephant_postgres") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddDbContext<PasswordAccountContext>();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// dependency injections
builder.Services.AddSingleton<EncryptionContext>();
builder.Services.AddScoped<IPasswordManagerAccountRepository<PasswordmanagerAccount>, PasswordManagerAccountRepository>();

// builder.Services.AddScoped<IPasswordAccountApiService<PasswordmanagerAccount>, PasswordAccountApiService>();

builder.Services.AddScoped(http => new HttpClient {
    BaseAddress = new Uri(builder.Configuration.GetSection("BaseAddress").Value!)
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// additional middleware
app.MapControllers();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Counter).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
