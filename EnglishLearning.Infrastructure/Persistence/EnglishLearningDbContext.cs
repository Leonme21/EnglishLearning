using EnglishLearning.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearning.Infrastructure.Persistence;

public class EnglishLearningDbContext : DbContext
{
    public EnglishLearningDbContext(DbContextOptions<EnglishLearningDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Leccion> Lecciones { get; set; }
    public DbSet<Ejercicio> Ejercicios { get; set; }
    public DbSet<RespuestaUsuario> RespuestasUsuarios { get; set; }
    public DbSet<ProgresoNivel> ProgresosNiveles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EnglishLearningDbContext).Assembly);
    }
}
