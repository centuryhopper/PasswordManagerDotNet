using Microsoft.AspNetCore.Authentication.Cookies;
using MVC_RazorComp_PasswordManager.Contexts;
using MVC_RazorComp_PasswordManager.Interfaces;
using MVC_RazorComp_PasswordManager.Repositories;

// TODO: change connection string before entering real data because previous connection string has already been committed to source control

// old users will never get deleted. The userrole pairs they had will for the sake of simplicity
// dotnet ef dbcontext scaffold "" Npgsql.EntityFrameworkCore.PostgreSQL -o Temp -t userroles -t passwordmanager_accounts -t passwordmanager_users -t roles -t usertokens

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
});

builder
    .Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.HttpOnly = true;
    });

builder.Services.Configure<CookieAuthenticationOptions>(config => {
    config.AccessDeniedPath = new PathString("/AccessDenied");
    config.Events = new CookieAuthenticationEvents
            {
                OnRedirectToAccessDenied = context =>
                {
                    // Custom logic when redirecting to access denied page
                    context.Response.Redirect("/AccessDenied");
                    return Task.CompletedTask;
                },
                // Add other event handlers as needed
            };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ADMIN", policy => policy.RequireRole("Admin"));
    options.AddPolicy("USER", policy => policy.RequireRole("User"));
});

builder.Services.AddServerSideBlazor();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<PasswordAccountContext>();

builder.Services.AddSingleton<EncryptionContext>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<
    IPasswordManagerAccountRepository<PasswordmanagerAccount>,
    PasswordManagerAccountRepository
>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapBlazorHub();

app.Run();
