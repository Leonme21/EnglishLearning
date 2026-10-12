using AspNetCoreRateLimit;
using EnglishLearning.Api.Middleware;

namespace EnglishLearning.Api.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder app)
    {
        app.UseIpRateLimiting();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<SecurityHeadersMiddleware>();

        return app;
    }
}