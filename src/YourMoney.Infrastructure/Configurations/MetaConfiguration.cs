using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;

namespace YourMoney.Infrastructure.Configurations
{
    public class MetaConfiguration : IEntityTypeConfiguration<Meta>
    {
        public void Configure(EntityTypeBuilder<Meta> builder)
        {
            builder.ToTable("tbMeta");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(m => m.Descricao)
                .HasMaxLength(500);

            builder.Property(m => m.ValorObjetivo);

            builder.Property(m => m.ValorAtual);

            builder.Property(m => m.DataInicio)
                .IsRequired();

            builder.Property(m => m.DataObjetivo)
                .IsRequired();

            builder.Property(m => m.Status)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(StatusMeta.Ativa);

            builder.HasOne(m => m.Categoria)
                .WithMany()
                .HasForeignKey(m => m.CategoriaId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(m => m.DataObjetivo)
                .HasDatabaseName("IX_Meta_DataObjetivo");

            builder.HasIndex(m => m.Status)
                .HasDatabaseName("IX_Meta_Status");
        }
    }
}