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
        builder.Property(g => g.Descricao).HasMaxLength(GrupoObjetivoInvestimento.DescricaoMaxima);
        builder.Property(g => g.ImagemUrl).HasMaxLength(GrupoObjetivoInvestimento.ImagemUrlMaxima);
        builder.Property(g => g.ValorAlvo).HasColumnType("decimal(18,2)");
        builder.Property(g => g.DataAlvo).HasColumnType("date");
        builder.Property(g => g.Ativo).IsRequired().HasDefaultValue(true);
        builder.Property(g => g.ArquivadoEm);
        builder.Property(g => g.Versao).IsRequired().IsRowVersion().IsConcurrencyToken();

        builder.HasOne(g => g.GrupoPai)
            .WithMany(g => g.Subobjetivos)
            .HasForeignKey(g => new { g.GrupoPaiId, g.UsuarioId })
            .HasPrincipalKey(g => new { g.Id, g.UsuarioId })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(g => new { g.UsuarioId, g.NomeNormalizado })
            .IsUnique()
            .HasDatabaseName("UX_InvestimentoGrupoObjetivo_UsuarioId_NomeNormalizado");
        builder.HasIndex(g => new { g.UsuarioId, g.Ativo });
    }
}
