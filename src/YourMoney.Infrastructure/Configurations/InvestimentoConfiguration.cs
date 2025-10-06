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

            //builder.Property(i => i.ValorInvestido);

            builder.Property(i => i.ValorAtual);

            builder.Property(i => i.DataInvestimento)
                .IsRequired();

            builder.Property(i => i.DataResgate);

            //builder.Property(i => i.TipoInvestimento)
            //    .IsRequired()
            //    .HasMaxLength(50);

            //builder.Property(i => i.TaxaRetorno)
            //    .HasColumnType("decimal(5,2)")
            //    .HasDefaultValue(0);

            builder.Property(i => i.Ativo)
                .IsRequired()
                .HasDefaultValue(true);

            builder.HasIndex(i => i.DataInvestimento)
                .HasDatabaseName("IX_Investimento_Data");

            //builder.HasIndex(i => i.TipoInvestimento)
            //    .HasDatabaseName("IX_Investimento_Tipo");
        }
    }
}