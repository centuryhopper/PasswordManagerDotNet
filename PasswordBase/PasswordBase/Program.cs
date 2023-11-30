using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PasswordBase.Client.Pages;
using PasswordBase.Components;
using PasswordBase.Contexts;
using PasswordBase.Shared.Models.IdentityModels;

// This project was created via this command: dotnet new blazor -o BlazorApp -int Auto
// which is per-component/page interactivity

// class library was created via the command: dotnet new classlib -n PasswordBase.Shared
// dotnet add reference PasswordBase.Shared/PasswordBase.Shared.csproj

// parse the elephantSQL provided string into ASP.net core friendly connection string
string getConnectionString(WebApplicationBuilder builder)
{
    // ElephantSQL formatting
    var optionsBuilder = new DbContextOptionsBuilder<PasswordDbContext>();
    // ElephantSQL formatting
    var uriString = builder.Configuration.GetConnectionString("cloudConnectionString")!;
    var uri = new Uri(uriString);
    var db = uri.AbsolutePath.Trim('/');
    var user = uri.UserInfo.Split(':')[0];
    var passwd = uri.UserInfo.Split(':')[1];
    var port = uri.Port > 0 ? uri.Port : 5432;
    var connStr = string.Format("Server={0};Database={1};User Id={2};Password={3};Port={4}",
        uri.Host, db, user, passwd, port);
    return connStr;
}

var builder = WebApplication.CreateBuilder(args);

// identity framework setup
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // must confirm email
    options.SignIn.RequireConfirmedEmail = true;

    // keep it stupid simple JUST for now
    options.Password.RequiredLength = 1;
    options.Password.RequiredUniqueChars = 1;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;

    // lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(30);
    options.Lockout.MaxFailedAccessAttempts = 10;


}).AddEntityFrameworkStores<PasswordDbContext>().AddDefaultTokenProviders();


// link to postgreSQL db for entity framework
builder.Services.AddDbContextPool<PasswordDbContext>(
    options =>
    {
        var connStr = getConnectionString(builder);
        options.UseNpgsql(
                connStr
        ).EnableSensitiveDataLogging();
    }
);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Counter).Assembly);

app.Run();
