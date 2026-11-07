using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourMoney.Domain.Entities;

namespace YourMoney.Infrastructure.Configurations;

public sealed class RegraNegocioUsuarioConfiguration : IEntityTypeConfiguration<RegraNegocioUsuario>
{
    public void Configure(EntityTypeBuilder<RegraNegocioUsuario> builder)
    {
        builder.ToTable("tbRegraNegocioUsuario");
        builder.HasKey(rule => rule.Id);
        builder.Property(rule => rule.UsuarioId).IsRequired().HasMaxLength(450);
        builder.Property(rule => rule.Codigo).IsRequired().HasMaxLength(100);
        builder.Property(rule => rule.Ativa).IsRequired();
        builder.Property(rule => rule.AtualizadaEm).IsRequired();
        builder.HasIndex(rule => new { rule.UsuarioId, rule.Codigo }).IsUnique();
    }
}
