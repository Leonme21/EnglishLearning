using System.Net;
using System.Net.Http.Headers;
using EnglishLearning.Client.Services.Auth;

namespace EnglishLearning.Client.Services.Api;

public sealed class AuthHeaderHandler(TokenStore tokens, SessionRefresh session) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var observedToken = await tokens.GetAccessTokenAsync();
        if (string.IsNullOrWhiteSpace(observedToken))
            return await base.SendAsync(request, ct);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", observedToken);
        var response = await base.SendAsync(request, ct);
        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        var access = await session.RenewAsync(observedToken, ct);
        if (string.IsNullOrWhiteSpace(access))
            return response;

        using var retry = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy
        };
        foreach (var header in request.Headers)
            retry.Headers.TryAddWithoutValidation(header.Key, header.Value);
        foreach (var option in request.Options)
            retry.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);
        if (request.Content is not null)
        {
            retry.Content = new ByteArrayContent(await request.Content.ReadAsByteArrayAsync(ct));
            foreach (var header in request.Content.Headers)
                retry.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", access);
        response.Dispose();
        return await base.SendAsync(retry, ct);
    }
}
