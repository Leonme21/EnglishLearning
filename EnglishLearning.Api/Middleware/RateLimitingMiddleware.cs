using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EnglishLearning.Api.Middleware;

public sealed class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IOptions<IpRateLimitOptions> _options;
    private readonly IOptions<IpRateLimitPolicies> _policies;

    public RateLimitingMiddleware(RequestDelegate next, IOptions<IpRateLimitOptions> options, IOptions<IpRateLimitPolicies> policies)
    {
        _next = next;
        _options = options;
        _policies = policies;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Rate limiting is handled by AspNetCoreRateLimit middleware
        // This is a placeholder for custom rate limiting logic if needed
        await _next(context);
    }
}