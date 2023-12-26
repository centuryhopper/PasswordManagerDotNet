

using System.Security.Claims;
using BlazorServerPasswordManager.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace BlazorServerPasswordManager.Authentication;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private ClaimsPrincipal _Anonymous = new(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(ProtectedSessionStorage sessionStorage)
    {
        this._sessionStorage = sessionStorage;
    }
    
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var userSessionStorageResult = await _sessionStorage.GetAsync<UserSession>("UserSession");
    
            UserSession? userSession = userSessionStorageResult.Success ? userSessionStorageResult.Value : null;
    
            if (userSession is null)
            {
                return await Task.FromResult(new AuthenticationState(_Anonymous));
            }
    
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
                new List<Claim>{
                    new(ClaimTypes.Name, userSession.UserName),
                    new(ClaimTypes.Role, userSession.Role),
            }, "CustomAuth"));
    
            return await Task.FromResult(new AuthenticationState(claimsPrincipal));
        }
        catch (System.Exception ex)
        {
            return await Task.FromResult(new AuthenticationState(_Anonymous));
        }
    }

    public async Task UpdateAuthenticationState(UserSession? userSession)
    {
        ClaimsPrincipal claimsPrincipal;

        if (userSession is not null)
        {
            await _sessionStorage.SetAsync("UserSession", userSession);
            claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>{
                new Claim(ClaimTypes.Name, userSession.UserName),
                new Claim(ClaimTypes.Name, userSession.Role),
            }));
        }
        else
        {
            await _sessionStorage.DeleteAsync("UserSession");
            claimsPrincipal = _Anonymous;
        }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
    }
}

