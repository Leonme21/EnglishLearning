using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishLearning.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Correo).IsRequired().HasMaxLength(150);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(512);
        builder.Property(u => u.RefreshToken).HasMaxLength(512);
        builder.Property(u => u.RefreshTokenExpiry);
        builder.Property(u => u.NivelActual).HasConversion<string>();
        builder.HasIndex(u => u.Correo).IsUnique();
    }
}

public class LeccionConfiguration : IEntityTypeConfiguration<Leccion>
{
    public void Configure(EntityTypeBuilder<Leccion> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Titulo).IsRequired().HasMaxLength(200);
        builder.Property(l => l.Descripcion).IsRequired().HasMaxLength(1000);
        builder.Property(l => l.NivelRequerido).HasConversion<string>();
        builder.HasIndex(l => l.NivelRequerido);

        builder.HasMany(l => l.Ejercicios)
               .WithOne(e => e.Leccion!)
               .HasForeignKey(e => e.LeccionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EjercicioConfiguration : IEntityTypeConfiguration<Ejercicio>
{
    public void Configure(EntityTypeBuilder<Ejercicio> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Contenido).IsRequired().HasMaxLength(4000);
        builder.Property(e => e.RespuestaCorrecta).IsRequired().HasMaxLength(2000);
        builder.Property(e => e.Tipo).HasConversion<string>();
        builder.HasIndex(e => e.LeccionId);
    }
}

public class RespuestaUsuarioConfiguration : IEntityTypeConfiguration<RespuestaUsuario>
{
    public void Configure(EntityTypeBuilder<RespuestaUsuario> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.RespuestaTexto).IsRequired().HasMaxLength(2000);
        builder.Property(r => r.FechaRealizacion).IsRequired();
        builder.HasIndex(r => r.UsuarioId);
        builder.HasIndex(r => r.EjercicioId);
        builder.HasIndex(r => r.FechaRealizacion);
    }
}

public class ProgresoNivelConfiguration : IEntityTypeConfiguration<ProgresoNivel>
{
    public void Configure(EntityTypeBuilder<ProgresoNivel> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nivel).HasConversion<string>();
        builder.Property(p => p.PorcentajeCompletado).HasPrecision(5, 2);
        builder.HasIndex(p => p.UsuarioId);
        builder.HasIndex(p => new { p.UsuarioId, p.Nivel }).IsUnique();
    }
}

public class PreguntaDiagnosticaConfiguration : IEntityTypeConfiguration<PreguntaDiagnostica>
{
    public void Configure(EntityTypeBuilder<PreguntaDiagnostica> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Contenido).IsRequired().HasMaxLength(4000);
        builder.Property(p => p.RespuestaCorrecta).IsRequired().HasMaxLength(2000);
        builder.Property(p => p.Habilidad).HasConversion<string>();
        builder.Property(p => p.Nivel).HasConversion<string>();
        builder.Property(p => p.OpcionesJson)
               .HasColumnType("jsonb")
               .IsRequired();

        builder.HasIndex(p => p.Habilidad);
        builder.HasIndex(p => p.Nivel);
    }
}

public class EvaluacionDiagnosticaConfiguration : IEntityTypeConfiguration<EvaluacionDiagnostica>
{
    public void Configure(EntityTypeBuilder<EvaluacionDiagnostica> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Fecha).IsRequired();
        builder.Property(e => e.NivelAsignado).HasConversion<string>();
        builder.Property(e => e.PuntajesPorHabilidadJson)
               .HasColumnType("jsonb")
               .IsRequired();

        builder.HasIndex(e => e.UsuarioId);
        builder.HasIndex(e => e.Fecha);
    }
}