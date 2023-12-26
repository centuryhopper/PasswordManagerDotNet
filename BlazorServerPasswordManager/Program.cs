using BlazorServerPasswordManager.Authentication;
using BlazorServerPasswordManager.Components;
using BlazorServerPasswordManager.Contexts;
using BlazorServerPasswordManager.Interfaces;
using BlazorServerPasswordManager.Repositories;
using Microsoft.AspNetCore.Components.Authorization;


/*
drop table "AspNetUserClaims";    
drop table "AspNetRoleClaims";    
drop table "AspNetUserRoles";     
drop table "AspNetUserTokens";    
drop table "AspNetUserLogins";    
drop table "AspNetUsers";         
drop table "AspNetRoles";         
drop table "__EFMigrationsHistory"

dotnet ef migrations add Initial -c ApplicationIdentityContext 
dotnet ef database update -c ApplicationIdentityContext        


dotnet ef dbcontext scaffold

*/

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationCore();

// var cs = builder.Configuration.GetConnectionString("elephant_postgres");
builder.Services.AddDbContext<PasswordAccountContext>();
// builder.Services.AddDbContextFactory<PasswordAccountContext>();

// builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddSingleton<EncryptionContext>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<IPasswordManagerAccountRepository<PasswordmanagerAccount>, PasswordManagerAccountRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();


app.MapRazorComponents<App>()
.AddInteractiveServerRenderMode();

app.Run();
