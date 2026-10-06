using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace EnglishLearning.Api.Extensions;

public static class SwaggerExtensions
{
    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "English Learning API v1");
            c.RoutePrefix = "swagger";
            c.DocumentTitle = "English Learning API Documentation";
            c.DefaultModelsExpandDepth(-1);
        });

        return app;
    }
}