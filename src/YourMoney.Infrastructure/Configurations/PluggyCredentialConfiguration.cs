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
        builder.HasIndex(credential => credential.UsuarioId).IsUnique();
    }
}
