using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace EnglishLearning.Client.Services.Auth;

public class JwtAuthStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly TokenStore _tokenStore;
    private readonly SessionRefresh _refresh;

    public JwtAuthStateProvider(TokenStore tokenStore, SessionRefresh refresh)
    {
        _tokenStore = tokenStore;
        _refresh = refresh;
        _refresh.Invalidated += NotifyUserLogout;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _tokenStore.GetAccessTokenAsync();
        var jwt = new JwtSecurityTokenHandler();
        if (token is null || !jwt.CanReadToken(token) || jwt.ReadJwtToken(token).ValidTo <= DateTime.UtcNow)
            token = await _refresh.RenewAsync(token);

        var identity = token is not null && jwt.CanReadToken(token) && jwt.ReadJwtToken(token).ValidTo > DateTime.UtcNow
            ? new ClaimsIdentity(jwt.ReadJwtToken(token).Claims, "jwt")
            : new ClaimsIdentity();
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public void NotifyUserAuthentication(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
        var authenticatedUser = new ClaimsPrincipal(identity);
        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        NotifyAuthenticationStateChanged(authState);
    }

    public void NotifyUserLogout()
    {
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = Task.FromResult(new AuthenticationState(anonymousUser));
        NotifyAuthenticationStateChanged(authState);
    }
    public void Dispose() => _refresh.Invalidated -= NotifyUserLogout;
}
