using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;

namespace YourMoney.Infrastructure.Configurations
{
    public class InvestimentoConfiguration : IEntityTypeConfiguration<Investimento>
    {
        public void Configure(EntityTypeBuilder<Investimento> builder)
        {
            builder.ToTable("tbInvestimento", table => table.HasCheckConstraint(
                "CK_Investimento_TipoAtivo",
                "[TipoAtivo] IS NULL OR [TipoAtivo] IN ('FundoInvestimento','Exterior','Criptomoeda','RendaFixa','Acoes','Outros')"));
            builder.HasKey(i => i.Id);

            builder.Property(i => i.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(i => i.Descricao)
                .HasMaxLength(500);

            builder.Property(i => i.Tipo)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(i => i.Quantidade)
                .HasColumnType("decimal(18,2)");

            builder.Property(i => i.PrecoMedio)
                .HasColumnType("decimal(18,2)");

            builder.Property(i => i.ValorAtual)
                .HasColumnType("decimal(18,2)");

            builder.Property(i => i.DataInvestimento)
                .IsRequired();

            builder.Property(i => i.MesReferencia)
                .HasColumnName("mesReferencia")
                .HasColumnType("date")
                .IsRequired(false);

            builder.Property(i => i.DataResgate);

            builder.Property(i => i.Ativo)
                .IsRequired()
                .HasDefaultValue(true);

            builder.HasOne(i => i.ReceitaRecorrente)
                .WithMany()
                .HasForeignKey(i => i.ReceitaRecorrenteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(i => i.TipoAtivo)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired(false);

            builder.HasOne(i => i.GrupoObjetivoInvestimento)
                .WithMany(g => g.Investimentos)
                .HasForeignKey(i => new { i.GrupoObjetivoInvestimentoId, i.UsuarioId })
                .HasPrincipalKey(g => new { g.Id, g.UsuarioId })
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(i => new { i.UsuarioId, i.DataInvestimento })
                .HasDatabaseName("IX_Investimento_UsuarioId_DataInvestimento");

            builder.HasIndex(i => new { i.UsuarioId, i.ReceitaRecorrenteId })
                .HasDatabaseName("IX_Investimento_UsuarioId_ReceitaRecorrenteId");

            builder.HasIndex(i => new { i.UsuarioId, i.GrupoObjetivoInvestimentoId })
                .HasDatabaseName("IX_Investimento_UsuarioId_GrupoObjetivoId");

            builder.HasIndex(i => new { i.UsuarioId, i.TipoAtivo })
                .HasDatabaseName("IX_Investimento_UsuarioId_TipoAtivo");

            builder.HasIndex(i => new { i.UsuarioId, i.OperacaoId })
                .IsUnique()
                .HasFilter("[OperacaoId] IS NOT NULL")
                .HasDatabaseName("UX_Investimento_UsuarioId_OperacaoId");

            builder.HasIndex(i => i.DataInvestimento)
                .HasDatabaseName("IX_Investimento_Data");

            builder.HasIndex(i => i.MesReferencia)
                .HasDatabaseName("IX_Investimento_MesReferencia");
        }
    }
}
