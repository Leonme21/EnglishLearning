using Microsoft.JSInterop;

namespace EnglishLearning.Client.Services.Auth;

public class TokenStore
{
    private const string AccessTokenKey = "jwt_access";
    private const string RefreshTokenKey = "jwt_refresh";
    private readonly IJSRuntime _jsRuntime;

    public TokenStore(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<string?> GetAccessTokenAsync() => await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", AccessTokenKey);
    public async Task<string?> GetRefreshTokenAsync() => await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", RefreshTokenKey);

    public async Task SetTokensAsync(string accessToken, string refreshToken)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", AccessTokenKey, accessToken);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", RefreshTokenKey, refreshToken);
    }

    public async Task ClearTokensAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", AccessTokenKey);
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", RefreshTokenKey);
    }
}
