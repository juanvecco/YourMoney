using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourMoney.Domain.Entities;

namespace YourMoney.Infrastructure.Configurations
{
    public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
    {
        public void Configure(EntityTypeBuilder<Categoria> builder)
        {
            builder.ToTable("tbCategoria");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Descricao)
                .HasMaxLength(500);

            builder.Property(c => c.Cor)
                .IsRequired()
                .HasMaxLength(7)
                .HasDefaultValue("#007bff");

            builder.Property(c => c.Icone)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("fa-circle");

            builder.Property(c => c.TipoTransacao)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(c => c.Ativa)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(c => c.DataCriacao)
                .IsRequired();

            builder.HasIndex(c => new { c.Nome, c.TipoTransacao })
                .IsUnique()
                .HasDatabaseName("IX_Categoria_Nome_Tipo");
        }
    }
}