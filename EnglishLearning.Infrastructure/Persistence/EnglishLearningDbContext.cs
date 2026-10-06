using EnglishLearning.Domain.Entities;
using EnglishLearning.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearning.Infrastructure.Persistence;

public class EnglishLearningDbContext : DbContext
{
    public EnglishLearningDbContext(DbContextOptions<EnglishLearningDbContext> options) : base(options) { }

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
}