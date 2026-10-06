using EnglishLearning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishLearning.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Token).IsRequired().HasMaxLength(512);
        builder.Property(r => r.Expiry).IsRequired();
        builder.Property(r => r.Revoked).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.RevokedAt);
        builder.Property(r => r.ReplacedByToken).HasMaxLength(512);
        
        builder.HasIndex(r => r.Token).IsUnique();
        builder.HasIndex(r => r.UsuarioId);
        builder.HasIndex(r => r.Expiry);
        
        builder.HasOne<Usuario>()
               .WithMany()
               .HasForeignKey(r => r.UsuarioId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}