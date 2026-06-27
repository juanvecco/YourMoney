using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourMoney.Domain.Entities;

namespace YourMoney.Infrastructure.Configurations
{
    public class MetaMensalConfiguration : IEntityTypeConfiguration<MetaMensal>
    {
        public void Configure(EntityTypeBuilder<MetaMensal> builder)
        {
            builder.ToTable("tbMetaMensal");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(m => m.PercentualReceita)
                .HasColumnType("decimal(9,4)")
                .IsRequired();

            builder.Property(m => m.MesReferencia)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(m => m.CriadoEm)
                .IsRequired();

            builder.Property(m => m.AtualizadoEm);

            builder.HasIndex(m => new { m.UsuarioId, m.MesReferencia })
                .HasDatabaseName("IX_MetaMensal_Usuario_MesReferencia");
        }
    }
}
