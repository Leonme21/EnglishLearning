using EnglishLearning.Application.Options;
using EnglishLearning.Domain.Interfaces;
using EnglishLearning.Domain.Options;
using EnglishLearning.Domain.Services;
using EnglishLearning.Infrastructure.Persistence;
using EnglishLearning.Infrastructure.Repositories.Lessons;
using EnglishLearning.Infrastructure.Repositories.Users;
using EnglishLearning.Infrastructure.Repositories.Assessments;
using EnglishLearning.Infrastructure.Repositories.Auth;
using EnglishLearning.Infrastructure.UnitOfWork;
using EnglishLearning.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EnglishLearning.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Options
        services.Configure<JwtOptions>(options => configuration.GetSection(JwtOptions.SectionName).Bind(options));
        services.Configure<SeedDataOptions>(options => configuration.GetSection(SeedDataOptions.SectionName).Bind(options));

        // Database
        var connectionString = configuration.GetConnectionString("Supabase")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'Supabase' or 'DefaultConnection' not found.");

        services.AddDbContext<EnglishLearningDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            }));

        // Repositories
        services.AddScoped<LeccionQueryRepository>();
        services.AddScoped<LeccionCommandRepository>();
        services.AddScoped<UsuarioQueryRepository>();
        services.AddScoped<UsuarioCommandRepository>();
        services.AddScoped<AssessmentQueryRepository>();
        services.AddScoped<AssessmentCommandRepository>();
        services.AddScoped<RefreshTokenQueryRepository>();
        services.AddScoped<RefreshTokenCommandRepository>();

        // Repository interfaces
        services.AddScoped<ILeccionQueryRepository>(sp => sp.GetRequiredService<LeccionQueryRepository>());
        services.AddScoped<ILeccionCommandRepository>(sp => sp.GetRequiredService<LeccionCommandRepository>());
        services.AddScoped<IUsuarioQueryRepository>(sp => sp.GetRequiredService<UsuarioQueryRepository>());
        services.AddScoped<IUsuarioCommandRepository>(sp => sp.GetRequiredService<UsuarioCommandRepository>());
        services.AddScoped<IAssessmentQueryRepository>(sp => sp.GetRequiredService<AssessmentQueryRepository>());
        services.AddScoped<IAssessmentCommandRepository>(sp => sp.GetRequiredService<AssessmentCommandRepository>());
        services.AddScoped<IRefreshTokenQueryRepository>(sp => sp.GetRequiredService<RefreshTokenQueryRepository>());
        services.AddScoped<IRefreshTokenCommandRepository>(sp => sp.GetRequiredService<RefreshTokenCommandRepository>());

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        // Services
        services.AddScoped<EnglishLearning.Domain.Services.IJwtTokenService, JwtTokenService>();
        services.AddScoped<EnglishLearning.Domain.Services.IPasswordHasher, PasswordHasher>();
        services.AddScoped<ISeedDataService, SeedDataService>();

        return services;
    }
}