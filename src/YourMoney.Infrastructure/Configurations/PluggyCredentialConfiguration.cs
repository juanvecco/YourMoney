using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourMoney.Domain.Entities;

namespace YourMoney.Infrastructure.Configurations;

public sealed class PluggyCredentialConfiguration : IEntityTypeConfiguration<PluggyCredential>
{
    public void Configure(EntityTypeBuilder<PluggyCredential> builder)
    {
        builder.ToTable("tbPluggyCredential");
        builder.HasKey(credential => credential.Id);
        builder.Property(credential => credential.ClientId).IsRequired().HasMaxLength(200);
        builder.Property(credential => credential.ProtectedClientSecret).IsRequired().HasMaxLength(2000);
        builder.Property(credential => credential.UpdatedAt).IsRequired();
        builder.Property(credential => credential.CarteiraId).IsRequired();
        builder.HasOne<Carteira>()
            .WithMany()
            .HasForeignKey(credential => new { credential.CarteiraId, credential.UsuarioId })
            .HasPrincipalKey(wallet => new { wallet.Id, wallet.UsuarioId })
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(credential => credential.CarteiraId).IsUnique();
    }
}
