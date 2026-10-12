using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Events;
using EnglishLearning.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearning.Infrastructure.Persistence;

public class EnglishLearningDbContext : DbContext
{
    private readonly IDomainEventDispatcher? _dispatcher;

    public EnglishLearningDbContext(
        DbContextOptions<EnglishLearningDbContext> options,
        IDomainEventDispatcher? dispatcher = null) : base(options)
    {
        _dispatcher = dispatcher;
    }

    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Leccion> Lecciones { get; set; } = null!;
    public DbSet<Ejercicio> Ejercicios { get; set; } = null!;
    public DbSet<RespuestaUsuario> RespuestasUsuarios { get; set; } = null!;
    public DbSet<ProgresoNivel> ProgresosNiveles { get; set; } = null!;
    public DbSet<PreguntaDiagnostica> PreguntasDiagnosticas { get; set; } = null!;
    public DbSet<EvaluacionDiagnostica> EvaluacionesDiagnosticas { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EnglishLearningDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql();
        }
        base.OnConfiguring(optionsBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEntities = ChangeTracker
            .Entries<AggregateRoot>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.DomainEvents)
            .ToList();

        foreach (var entity in domainEntities)
        {
            entity.ClearDomainEvents();
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        if (_dispatcher != null && domainEvents.Count > 0)
        {
            await _dispatcher.DispatchAsync(domainEvents, cancellationToken);
        }

        return result;
    }
}