using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourMoney.Domain.Entities;

namespace YourMoney.Infrastructure.Configurations
{
    public class InvestimentoConfiguration : IEntityTypeConfiguration<Investimento>
    {
        public void Configure(EntityTypeBuilder<Investimento> builder)
        {
            builder.ToTable("tbInvestimento");
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

            builder.HasIndex(i => i.DataInvestimento)
                .HasDatabaseName("IX_Investimento_Data");

            builder.HasIndex(i => i.MesReferencia)
                .HasDatabaseName("IX_Investimento_MesReferencia");
        }
    }
}
