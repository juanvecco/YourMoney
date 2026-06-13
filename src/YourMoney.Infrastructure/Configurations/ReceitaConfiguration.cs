using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;

namespace YourMoney.Infrastructure.Configurations
{
    public class ReceitaConfiguration : IEntityTypeConfiguration<Receita>
    {
        public void Configure(EntityTypeBuilder<Receita> builder)
        {
            builder.ToTable("tbReceita");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Descricao)
                .IsRequired()
                .HasMaxLength(255);

            // Fix: Use the correct type for the 'Valor' property, assuming it's a ValueObject  
            builder.Property(r => r.Valor);

            builder.Property(r => r.Data)
                .IsRequired();

            builder.Property(r => r.MesReferencia)
                .HasColumnName("mesReferencia")
                .HasColumnType("date")
                .IsRequired(false);
        }
    }
}
