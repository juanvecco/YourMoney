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

            builder.Property(c => c.Descricao)
                .HasMaxLength(500)
                .IsRequired(false); // Permitido ser NULL no banco

            builder.Property(c => c.TipoTransacao)
                .IsRequired(true); // Ajustado conforme o banco

            builder.Property(c => c.CategoriaPaiId)
                .IsRequired(false); // Autorrelacionamento opcional

            //builder.HasOne(c => c.CategoriaPai)
            //    .WithMany(c => c.Subcategorias)
            //    .HasForeignKey(c => c.CategoriaPaiId)
            //    .OnDelete(DeleteBehavior.Restrict); // ou .SetNull se preferir

            builder.HasIndex(c => c.CategoriaPaiId)
                .HasDatabaseName("IX_Categoria_CategoriaPaiId");
        }
    }
}
