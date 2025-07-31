using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;

namespace YourMoney.Infrastructure.Configurations
{
    public class DespesaConfiguration : IEntityTypeConfiguration<Despesa>
    {
        public void Configure(EntityTypeBuilder<Despesa> builder)
        {
            builder.ToTable("tbDespesa");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Descricao)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(d => d.Valor);

            builder.Property(d => d.Data).IsRequired();

            builder.Property(d => d.IdContaFinanceira).IsRequired();

            builder.HasOne(d => d.ContaFinanceira)
                .WithMany()
                .HasForeignKey(d => d.IdContaFinanceira)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(d => d.IdContaFinanceira)
                .HasDatabaseName("IX_Despesa_IdContaFinanceira");

            builder.Property(d => d.IdCategoria).IsRequired();

            builder.HasOne(d => d.Categoria)
                .WithMany()
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(d => d.IdCategoria)
                .HasDatabaseName("IX_Despesa_IdCategoria");
            //builder.Property(d => d.Pago).IsRequired().HasDefaultValue(false);
            //builder.Property(d => d.DataPagamento);
            //builder.Property(d => d.TipoRecorrencia)
            //    .IsRequired()
            //    .HasConversion<int>()
            //    .HasDefaultValue(TipoRecorrencia.Unica);
            //builder.Property(d => d.DataCriacao).IsRequired();

            //builder.HasOne(d => d.Categoria)
            //    .WithMany()
            //    .HasForeignKey(d => d.CategoriaId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //builder.HasIndex(d => d.Data).HasDatabaseName("IX_Despesa_Data");
            //builder.HasIndex(d => d.CategoriaId).HasDatabaseName("IX_Despesa_Categoria"); 
            //builder.HasIndex(d => d.TipoRecorrencia).HasDatabaseName("IX_Despesa_TipoRecorrencia");
        }
    }
}
