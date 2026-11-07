using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourMoney.Domain.Entities;

namespace YourMoney.Infrastructure.Configurations;

public sealed class CarteiraConfiguration : IEntityTypeConfiguration<Carteira>
{
    public void Configure(EntityTypeBuilder<Carteira> builder)
    {
        builder.ToTable("tbCarteira");
        builder.HasKey(wallet => wallet.Id);
        builder.Property(wallet => wallet.UsuarioId).IsRequired().HasMaxLength(450);
        builder.Property(wallet => wallet.Nome).IsRequired().HasMaxLength(Carteira.NomeMaximo);
        builder.Property(wallet => wallet.NomeNormalizado).IsRequired().HasMaxLength(Carteira.NomeMaximo);
        builder.Property(wallet => wallet.Principal).IsRequired();
        builder.Property(wallet => wallet.CriadaEm).IsRequired();
        builder.HasAlternateKey(wallet => new { wallet.Id, wallet.UsuarioId });
        builder.HasIndex(wallet => new { wallet.UsuarioId, wallet.NomeNormalizado }).IsUnique();
        builder.HasIndex(wallet => wallet.UsuarioId)
            .IsUnique()
            .HasFilter("[Principal] = 1");
    }
}
