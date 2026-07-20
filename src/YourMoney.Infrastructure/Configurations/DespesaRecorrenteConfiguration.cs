using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourMoney.Domain.Entities;

namespace YourMoney.Infrastructure.Configurations
{
    public class DespesaRecorrenteConfiguration : IEntityTypeConfiguration<DespesaRecorrente>
    {
        public void Configure(EntityTypeBuilder<DespesaRecorrente> builder)
        {
            builder.ToTable("tbDespesaRecorrente");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Descricao)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(r => r.ValorPrevisto)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(r => r.DiaVencimento).IsRequired();
            builder.Property(r => r.DataInicio).IsRequired();
            builder.Property(r => r.DataTermino).IsRequired(false);
            builder.Property(r => r.Ativa).IsRequired();
            builder.Property(r => r.CriadoEm).IsRequired();
            builder.Property(r => r.AtualizadoEm).IsRequired();

            builder.HasOne(r => r.ContaFinanceira)
                .WithMany()
                .HasForeignKey(r => r.IdContaFinanceira)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.TipoDespesa)
                .WithMany()
                .HasForeignKey(r => r.IdTipoDespesa)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.NaturezaDespesa)
                .WithMany()
                .HasForeignKey(r => r.IdNaturezaDespesa)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Categoria)
                .WithMany()
                .HasForeignKey(r => r.IdCategoria)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(r => new { r.UsuarioId, r.Ativa })
                .HasDatabaseName("IX_DespesaRecorrente_UsuarioId_Ativa");

            builder.HasIndex(r => new { r.UsuarioId, r.DataInicio, r.DataTermino })
                .HasDatabaseName("IX_DespesaRecorrente_UsuarioId_Periodo");

            builder.HasIndex(r => r.IdContaFinanceira)
                .HasDatabaseName("IX_DespesaRecorrente_IdContaFinanceira");

            builder.HasIndex(r => r.IdTipoDespesa)
                .HasDatabaseName("IX_DespesaRecorrente_IdTipoDespesa");

            builder.HasIndex(r => r.IdNaturezaDespesa)
                .HasDatabaseName("IX_DespesaRecorrente_IdNaturezaDespesa");

            builder.HasIndex(r => r.IdCategoria)
                .HasDatabaseName("IX_DespesaRecorrente_IdCategoria");
        }
    }
}
