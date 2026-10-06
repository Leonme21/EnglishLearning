using EnglishLearning.Client.Models;
using EnglishLearning.Client.Services.Api;
using Microsoft.AspNetCore.Components.Authorization;

namespace EnglishLearning.Client.Services.Auth;

public class AuthService
{
    private readonly ApiClient _apiClient;
    private readonly TokenStore _tokenStore;
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthService(ApiClient apiClient, TokenStore tokenStore, AuthenticationStateProvider authStateProvider)
    {
        _apiClient = apiClient;
        _tokenStore = tokenStore;
        _authStateProvider = authStateProvider;
    }

    public async Task<ApiResult<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var result = await _apiClient.PostAsync<AuthResponse>("api/v1/auth/login", request);
        if (result.IsSuccess && result.Data != null)
        {
            await _tokenStore.SetTokensAsync(result.Data.AccessToken, result.Data.RefreshToken);
            ((JwtAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Data.AccessToken);
        }
        return result;
    }

    public async Task<ApiResult<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var result = await _apiClient.PostAsync<AuthResponse>("api/v1/auth/register", request);
        if (result.IsSuccess && result.Data != null)
        {
            await _tokenStore.SetTokensAsync(result.Data.AccessToken, result.Data.RefreshToken);
            ((JwtAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Data.AccessToken);
        }
        return result;
    }

    public async Task LogoutAsync()
    {
        var refreshToken = await _tokenStore.GetRefreshTokenAsync();
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            // Omitimos el error si el revoke falla (e.g. token ya expirado)
            await _apiClient.PostAsync<object>("api/v1/auth/revoke", new RevokeTokenRequest(refreshToken));
        }

        await _tokenStore.ClearTokensAsync();
        ((JwtAuthStateProvider)_authStateProvider).NotifyUserLogout();
    }
}
