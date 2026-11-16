using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourMoney.Domain.Entities;

namespace YourMoney.Infrastructure.Configurations;

public sealed class FaturaContaConfiguration : IEntityTypeConfiguration<FaturaConta>
{
    public void Configure(EntityTypeBuilder<FaturaConta> builder)
    {
        builder.ToTable("tbFaturaConta");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.MesReferencia).IsRequired();
        builder.Property(f => f.ValorInformado).HasColumnType("decimal(18,2)").IsRequired();
        builder.HasOne(f => f.ContaFinanceira).WithMany()
            .HasForeignKey(f => f.IdContaFinanceira).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(f => new { f.UsuarioId, f.IdContaFinanceira, f.MesReferencia }).IsUnique();
    }
}
