using EnglishLearning.Api.Extensions;
using EnglishLearning.Api.Middleware;
using EnglishLearning.Infrastructure.Persistence;
using EnglishLearning.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add API services (controllers, swagger, auth, cors, rate limiting, health checks, application, infrastructure)
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("EnableSwagger"))
{
    app.UseSwaggerDocumentation();
    app.UseDeveloperExceptionPage();
}

app.UseCustomMiddleware();

// Serve Blazor WebAssembly client
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapFallbackToFile("index.html");

// Auto-migrate and seed data on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<EnglishLearningDbContext>();
        await dbContext.Database.MigrateAsync();

        var seedService = scope.ServiceProvider.GetRequiredService<ISeedDataService>();
        await seedService.SeedAsync();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error occurred while migrating or seeding the database.");
        throw;
    }
}

app.Run();