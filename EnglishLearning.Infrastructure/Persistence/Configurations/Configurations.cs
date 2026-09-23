using EnglishLearning.Domain.Entities;
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
        builder.Property(u => u.NivelActual).HasConversion<string>();
    }
}

public class LeccionConfiguration : IEntityTypeConfiguration<Leccion>
{
    public void Configure(EntityTypeBuilder<Leccion> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Titulo).IsRequired().HasMaxLength(200);
        builder.Property(l => l.NivelRequerido).HasConversion<string>();
        
        builder.HasMany(l => l.Ejercicios)
               .WithOne(e => e.Leccion)
               .HasForeignKey(e => e.LeccionId);
    }
}

public class EjercicioConfiguration : IEntityTypeConfiguration<Ejercicio>
{
    public void Configure(EntityTypeBuilder<Ejercicio> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Contenido).IsRequired();
        builder.Property(e => e.RespuestaCorrecta).IsRequired();
        builder.Property(e => e.Tipo).HasConversion<string>();
    }
}
