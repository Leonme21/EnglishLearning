using System.Net;
using System.Net.Http.Json;
using EnglishLearning.Client.Models;
using Microsoft.Extensions.Logging;

namespace EnglishLearning.Client.Services.Auth;

public sealed class SessionRefresh(TokenStore tokens, IHttpClientFactory clients, ILogger<SessionRefresh> logger)
{
    public event Action? Invalidated;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task<string?> RenewAsync(string? observedToken, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var current = await tokens.GetAccessTokenAsync();
            if (current != observedToken)
                return current;

            var refresh = await tokens.GetRefreshTokenAsync();
            if (string.IsNullOrWhiteSpace(refresh))
            {
                await tokens.ClearTokensAsync();
                return null;
            }

            using var client = clients.CreateClient("AuthClient");
            using var reply = await client.PostAsJsonAsync("api/v1/auth/refresh",
                new RefreshTokenRequest(refresh), cancellationToken);

            if (reply.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest)
            {
                await tokens.ClearTokensAsync();
                Invalidated?.Invoke();
            }
            if (!reply.IsSuccessStatusCode)
                return null;

            var body = await reply.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>(cancellationToken);
            if (body?.Data?.AccessToken is not { Length: > 0 } access ||
                body.Data.RefreshToken is not { Length: > 0 } rotated)
                return null;

            await tokens.SetTokensAsync(access, rotated);
            return access;
        }
        catch (HttpRequestException exception)
        {
            logger.LogDebug("No se pudo renovar la sesión. Estado: {Status}", exception.StatusCode);
            return null;
        }
        finally
        {
            _gate.Release();
        }
    }
}
