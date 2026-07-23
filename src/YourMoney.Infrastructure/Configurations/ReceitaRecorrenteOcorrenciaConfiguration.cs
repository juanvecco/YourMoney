using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourMoney.Domain.Entities;

namespace YourMoney.Infrastructure.Configurations
{
    public class ReceitaRecorrenteOcorrenciaConfiguration : IEntityTypeConfiguration<ReceitaRecorrenteOcorrencia>
    {
        public void Configure(EntityTypeBuilder<ReceitaRecorrenteOcorrencia> builder)
        {
            builder.ToTable("tbReceitaRecorrenteOcorrencia");
            builder.HasKey(o => o.Id);

            builder.Property(o => o.MesReferencia)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(o => o.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.HasOne(o => o.ReceitaConfirmada)
                .WithMany()
                .HasForeignKey(o => o.ReceitaConfirmadaId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(o => new { o.UsuarioId, o.ReceitaRecorrenteId, o.MesReferencia })
                .IsUnique()
                .HasDatabaseName("UX_ReceitaRecorrenteOcorrencia_Usuario_Recorrencia_Mes");

            builder.HasIndex(o => new { o.UsuarioId, o.MesReferencia, o.Status })
                .HasDatabaseName("IX_ReceitaRecorrenteOcorrencia_Usuario_Mes_Status");
        }
    }
}
