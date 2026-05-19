using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using YourMoney.Domain.Entities;

namespace YourMoney.Infrastructure.Configurations
{
    public class TipoDespesaConfiguration : IEntityTypeConfiguration<TipoDespesa>
    {
        public void Configure(EntityTypeBuilder<TipoDespesa> builder)
        {
            builder.ToTable("tbTipoDespesa");

            builder.HasKey(t => t.idTipoDespesa);

            builder.Property(t => t.txtDescricao)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasData(
                new TipoDespesa(1, "Essencial"),
                new TipoDespesa(2, "Lazer")
            );
        }
    }
}
