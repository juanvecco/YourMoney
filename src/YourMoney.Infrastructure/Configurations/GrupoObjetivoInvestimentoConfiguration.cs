using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourMoney.Domain.Entities;

namespace YourMoney.Infrastructure.Configurations;

public sealed class GrupoObjetivoInvestimentoConfiguration : IEntityTypeConfiguration<GrupoObjetivoInvestimento>
{
    public void Configure(EntityTypeBuilder<GrupoObjetivoInvestimento> builder)
    {
        builder.ToTable("tbInvestimentoGrupoObjetivo");
        builder.HasKey(g => g.Id);
        builder.HasAlternateKey(g => new { g.Id, g.UsuarioId });

        builder.Property(g => g.Nome).IsRequired().HasMaxLength(GrupoObjetivoInvestimento.NomeMaximo);
        builder.Property(g => g.NomeNormalizado).IsRequired().HasMaxLength(GrupoObjetivoInvestimento.NomeMaximo);
        builder.Property(g => g.Versao).IsRequired().IsRowVersion().IsConcurrencyToken();

        builder.HasIndex(g => new { g.UsuarioId, g.NomeNormalizado })
            .IsUnique()
            .HasDatabaseName("UX_InvestimentoGrupoObjetivo_UsuarioId_NomeNormalizado");
    }
}
