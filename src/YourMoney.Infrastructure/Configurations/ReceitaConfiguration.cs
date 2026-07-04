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

            // Fix: Use the correct type for the 'Valor' property, assuming it's a ValueObject  
            builder.Property(r => r.Valor)
                .HasColumnType("decimal(18,2)");

            builder.Property(r => r.Data)
                .IsRequired();

            builder.Property(r => r.MesReferencia)
                .HasColumnName("mesReferencia")
                .HasColumnType("date")
                .IsRequired(false);

            builder.Property(r => r.Natureza)
                .HasConversion<string>()
                .HasMaxLength(40)
                .IsRequired()
                .HasDefaultValue(NaturezaReceita.RendaDisponivel);

            builder.Property(r => r.DespesaVinculadaId)
                .IsRequired(false);

            builder.HasOne(r => r.DespesaVinculada)
                .WithMany()
                .HasForeignKey(r => r.DespesaVinculadaId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(r => r.DespesaVinculadaId)
                .HasDatabaseName("IX_Receita_DespesaVinculadaId");

            builder.HasIndex(r => new { r.UsuarioId, r.Natureza, r.MesReferencia })
                .HasDatabaseName("IX_Receita_Usuario_Natureza_MesReferencia");
        }
    }
}
