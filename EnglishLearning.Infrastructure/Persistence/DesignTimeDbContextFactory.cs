using EnglishLearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace EnglishLearning.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<EnglishLearningDbContext>
{
    public EnglishLearningDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        
        // Navigate to Api project for appsettings
        var apiPath = Path.GetFullPath(Path.Combine(basePath, "..", "EnglishLearning.Api"));
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiPath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration.GetConnectionString("Supabase")
            ?? "Host=localhost;Database=englishlearning;Username=postgres;Password=postgres;Port=5432";

        var optionsBuilder = new DbContextOptionsBuilder<EnglishLearningDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new EnglishLearningDbContext(optionsBuilder.Options);
    }
}