using System.Net.Http.Headers;
using System.Net.Http.Json;
using EnglishLearning.Client.Models;
using EnglishLearning.Client.Services.Auth;
using Microsoft.AspNetCore.Components.Authorization;

namespace EnglishLearning.Client.Services.Api;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly TokenStore _tokenStore;
    private readonly IServiceProvider _serviceProvider;
    private bool _isRefreshing = false;

    public AuthHeaderHandler(TokenStore tokenStore, IServiceProvider serviceProvider)
    {
        _tokenStore = tokenStore;
        _serviceProvider = serviceProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenStore.GetAccessTokenAsync();

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && !string.IsNullOrWhiteSpace(token) && !_isRefreshing)
        {
            _isRefreshing = true;
            try
            {
                var refreshToken = await _tokenStore.GetRefreshTokenAsync();
                if (!string.IsNullOrWhiteSpace(refreshToken))
                {
                    var httpClientFactory = (IHttpClientFactory)_serviceProvider.GetService(typeof(IHttpClientFactory))!;
                    var client = httpClientFactory.CreateClient("AuthClient");
                    
                    var refreshReq = new RefreshTokenRequest(token, refreshToken);
                    var refreshResponse = await client.PostAsJsonAsync("api/v1/auth/refresh", refreshReq, cancellationToken);
                    
                    if (refreshResponse.IsSuccessStatusCode)
                    {
                        var result = await refreshResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>(cancellationToken: cancellationToken);
                        if (result != null && result.Success && result.Data != null)
                        {
                            await _tokenStore.SetTokensAsync(result.Data.AccessToken, result.Data.RefreshToken);
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.Data.AccessToken);
                            // Re-enviar la solicitud con el nuevo token
                            return await base.SendAsync(request, cancellationToken);
                        }
                    }
                }
                
                await _tokenStore.ClearTokensAsync();
                var authStateProvider = (JwtAuthStateProvider)_serviceProvider.GetService(typeof(AuthenticationStateProvider))!;
                authStateProvider.NotifyUserLogout();
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        return response;
    }
}
