using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourMoney.Domain.Entities;

namespace YourMoney.Infrastructure.Configurations
{
    public class ReceitaRecorrenteConfiguration : IEntityTypeConfiguration<ReceitaRecorrente>
    {
        public void Configure(EntityTypeBuilder<ReceitaRecorrente> builder)
        {
            builder.ToTable("tbReceitaRecorrente");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Descricao)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(r => r.ValorPrevisto)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(r => r.Natureza)
                .HasConversion<string>()
                .HasMaxLength(40)
                .IsRequired();

            builder.Property(r => r.DataInicio).HasColumnType("date");
            builder.Property(r => r.DataTermino).HasColumnType("date").IsRequired(false);

            builder.HasOne(r => r.ContaFinanceira)
                .WithMany()
                .HasForeignKey(r => r.IdContaFinanceira)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(r => r.Ocorrencias)
                .WithOne(o => o.ReceitaRecorrente)
                .HasForeignKey(o => o.ReceitaRecorrenteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(r => new { r.UsuarioId, r.Ativa })
                .HasDatabaseName("IX_ReceitaRecorrente_UsuarioId_Ativa");

            builder.HasIndex(r => new { r.UsuarioId, r.DataInicio, r.DataTermino })
                .HasDatabaseName("IX_ReceitaRecorrente_UsuarioId_Periodo");
        }
    }
}
