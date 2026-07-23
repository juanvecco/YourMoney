using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourMoney.Domain.Entities;

namespace YourMoney.Infrastructure.Configurations
{
    public class DespesaRecorrenteOcorrenciaConfiguration : IEntityTypeConfiguration<DespesaRecorrenteOcorrencia>
    {
        public void Configure(EntityTypeBuilder<DespesaRecorrenteOcorrencia> builder)
        {
            builder.ToTable("tbDespesaRecorrenteOcorrencia");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.MesReferencia).IsRequired();

            builder.Property(o => o.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(o => o.FinalizadaEm).IsRequired(false);
            builder.Property(o => o.CriadoEm).IsRequired();

            builder.HasOne(o => o.DespesaRecorrente)
                .WithMany(r => r.Ocorrencias)
                .HasForeignKey(o => o.DespesaRecorrenteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(o => o.DespesaConfirmada)
                .WithMany()
                .HasForeignKey(o => o.DespesaConfirmadaId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(o => new { o.UsuarioId, o.DespesaRecorrenteId, o.MesReferencia })
                .IsUnique()
                .HasDatabaseName("UX_DespesaRecorrenteOcorrencia_Usuario_Recorrencia_Mes");

            builder.HasIndex(o => new { o.UsuarioId, o.MesReferencia, o.Status })
                .HasDatabaseName("IX_DespesaRecorrenteOcorrencia_Usuario_Mes_Status");

            builder.HasIndex(o => o.DespesaConfirmadaId)
                .HasDatabaseName("IX_DespesaRecorrenteOcorrencia_DespesaConfirmadaId");
        }
    }
}
