using EnglishLearning.Application.Common;
using EnglishLearning.Application.Features.Lessons.Commands;
using EnglishLearning.Application.Features.Lessons.Queries;
using EnglishLearning.Application.Features.Lessons.Validators;
using EnglishLearning.Application.Features.Assessments.Commands;
using EnglishLearning.Application.Features.Assessments.Queries;
using EnglishLearning.Application.Features.Assessments.Validators;
using EnglishLearning.Application.Features.Users.Commands;
using EnglishLearning.Application.Features.Users.Queries;
using EnglishLearning.Application.Features.Auth.Commands;
using EnglishLearning.Application.Mappings;
using EnglishLearning.Domain.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishLearning.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

        // Mapster
        MappingConfig.RegisterMappings();

        // Pipeline Behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        // Domain Services
        services.AddScoped<ILevelCalculator, LevelCalculator>();
        services.AddScoped<IRecommendationEngine, RecommendationEngine>();
        services.AddScoped<IExerciseEvaluator, ExerciseEvaluator>();

        return services;
    }
}