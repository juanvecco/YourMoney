using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourMoney.Domain.Entities;

namespace YourMoney.Infrastructure.Configurations;

public sealed class AporteObjetivoInvestimentoConfiguration : IEntityTypeConfiguration<AporteObjetivoInvestimento>
{
    public void Configure(EntityTypeBuilder<AporteObjetivoInvestimento> builder)
    {
        builder.ToTable("tbAporteObjetivoInvestimento", t => t.HasCheckConstraint("CK_AporteObjetivo_Valor", "[Valor] > 0"));
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Valor).HasColumnType("decimal(18,2)");
        builder.Property(a => a.MesReferencia).HasColumnType("date");
        builder.Property(a => a.DataConfirmacao).HasColumnType("date");
        builder.HasIndex(a => new { a.UsuarioId, a.OperacaoId }).IsUnique();
        builder.HasIndex(a => new { a.UsuarioId, a.MesReferencia });
        builder.HasIndex(a => new { a.UsuarioId, a.GrupoObjetivoInvestimentoId });
        builder.HasOne(a => a.MetaMensal).WithMany(m => m.AportesObjetivo)
            .HasForeignKey(a => new { a.MetaMensalId, a.UsuarioId }).HasPrincipalKey(m => new { m.Id, m.UsuarioId }).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.GrupoObjetivoInvestimento).WithMany(g => g.Aportes)
            .HasForeignKey(a => new { a.GrupoObjetivoInvestimentoId, a.UsuarioId }).HasPrincipalKey(g => new { g.Id, g.UsuarioId }).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Investimento).WithMany()
            .HasForeignKey(a => new { a.InvestimentoId, a.UsuarioId }).HasPrincipalKey(i => new { i.Id, i.UsuarioId }).OnDelete(DeleteBehavior.Restrict);
    }
}
