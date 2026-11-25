using EnglishLearning.Client.Models;
using EnglishLearning.Client.Services.Api;
using Microsoft.AspNetCore.Components.Authorization;

namespace EnglishLearning.Client.Services.Auth;

public class AuthService
{
    private readonly ApiClient _apiClient;
    private readonly TokenStore _tokenStore;
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthService(TokenStore tokenStore, AuthenticationStateProvider authStateProvider, IHttpClientFactory clients)
    {
        _apiClient = new ApiClient(clients.CreateClient("AuthClient"));
        _tokenStore = tokenStore;
        _authStateProvider = authStateProvider;
    }

    public async Task<ApiResult<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var result = await _apiClient.PostAsync<AuthResponse>("api/v1/auth/login", request);
        if (result.IsSuccess && result.Data != null)
        {
            if (!result.Data.RequiresOtp && result.Data.AccessToken is { Length: > 0 } && result.Data.RefreshToken is { Length: > 0 })
            {
                await _tokenStore.SetTokensAsync(result.Data.AccessToken, result.Data.RefreshToken);
                ((JwtAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Data.AccessToken);
            }
        }
        return result;
    }

    public async Task<ApiResult<AuthResponse>> VerifyOtpAsync(VerifyOtpRequest request)
    {
        var result = await _apiClient.PostAsync<AuthResponse>("api/v1/auth/verify-otp", request);
        if (result.IsSuccess && result.Data != null && result.Data.AccessToken is { Length: > 0 } && result.Data.RefreshToken is { Length: > 0 })
        {
            await _tokenStore.SetTokensAsync(result.Data.AccessToken, result.Data.RefreshToken);
            ((JwtAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Data.AccessToken);
        }
        return result;
    }

    public async Task<ApiResult<bool>> ResendOtpAsync(ResendOtpRequest request)
    {
        return await _apiClient.PostAsync<bool>("api/v1/auth/resend-otp", request);
    }

    public Task<ApiResult<AuthResponse>> RegisterAsync(RegisterRequest request) =>
        _apiClient.PostAsync<AuthResponse>("api/v1/auth/register", request);

    public Task<ApiResult<AuthResponse>> RequestPasswordResetAsync(string email) =>
        _apiClient.PostAsync<AuthResponse>("api/v1/auth/request-password-reset", new { email });

    public Task<ApiResult<bool>> ResetPasswordAsync(string challengeId, string code, string password) =>
        _apiClient.PostAsync<bool>("api/v1/auth/reset-password", new { challengeId, code, password });

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
