using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;

namespace YourMoney.Infrastructure.Configurations
{
    public class MetaMensalConfiguration : IEntityTypeConfiguration<MetaMensal>
    {
        public void Configure(EntityTypeBuilder<MetaMensal> builder)
        {
            builder.ToTable("tbMetaMensal", table =>
                table.HasCheckConstraint(
                    "CK_MetaMensal_Definicao",
                    "([TipoDefinicao] = N'Percentual' AND [PercentualReceita] IS NOT NULL AND [PercentualReceita] > 0 AND [ValorMeta] IS NULL) OR " +
                    "([TipoDefinicao] = N'Valor' AND [ValorMeta] IS NOT NULL AND [ValorMeta] > 0 AND [PercentualReceita] IS NULL)"));

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(m => m.TipoDefinicao)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(TipoDefinicaoMeta.Percentual)
                .IsRequired();

            builder.Property(m => m.PercentualReceita)
                .HasColumnType("decimal(9,4)")
                .IsRequired(false);

            builder.Property(m => m.ValorMeta)
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);

            builder.Property(m => m.MesReferencia)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(m => m.CriadoEm)
                .IsRequired();

            builder.Property(m => m.AtualizadoEm);

            builder.HasIndex(m => new { m.UsuarioId, m.MesReferencia })
                .HasDatabaseName("IX_MetaMensal_Usuario_MesReferencia");
        }
    }
}
