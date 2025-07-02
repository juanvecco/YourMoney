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

            builder.OwnsOne(r => r.Valor, money =>
            {
                money.Property(m => m.Valor)
                    .HasColumnName("Valor")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                money.Property(m => m.Moeda)
                    .HasColumnName("Moeda")
                    .HasMaxLength(3)
                    .IsRequired()
                    .HasDefaultValue("BRL");
            });

            builder.Property(r => r.Data)
                .IsRequired();

            builder.Property(r => r.Recebida)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(r => r.DataRecebimento);

            builder.Property(r => r.TipoRecorrencia)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(TipoRecorrencia.Unica);

            builder.Property(r => r.DataCriacao)
                .IsRequired();

            builder.HasOne(r => r.Categoria)
                .WithMany()
                .HasForeignKey(r => r.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(r => r.Data)
                .HasDatabaseName("IX_Receita_Data");

            builder.HasIndex(r => r.CategoriaId)
                .HasDatabaseName("IX_Receita_Categoria");
        }
    }
}