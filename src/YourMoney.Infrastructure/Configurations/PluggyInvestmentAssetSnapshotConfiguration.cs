using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourMoney.Domain.Entities;

namespace YourMoney.Infrastructure.Configurations;

public sealed class PluggyInvestmentAssetSnapshotConfiguration : IEntityTypeConfiguration<PluggyInvestmentAssetSnapshot>
{
    public void Configure(EntityTypeBuilder<PluggyInvestmentAssetSnapshot> builder)
    {
        builder.ToTable("tbPluggyInvestmentAssetSnapshot");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PluggyItemId).IsRequired().HasMaxLength(36);
        builder.Property(x => x.ExternalInvestmentId).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Type).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Subtype).HasMaxLength(100);
        builder.Property(x => x.Code).HasMaxLength(100);
        builder.Property(x => x.Isin).HasMaxLength(20);
        builder.Property(x => x.Balance).HasColumnType("decimal(18,2)");
        builder.Property(x => x.CurrencyCode).IsRequired().HasMaxLength(3);
        builder.Property(x => x.Quantity).HasColumnType("decimal(18,8)");
        builder.Property(x => x.CurrentUnitQuote).HasColumnType("decimal(18,8)");
        builder.Property(x => x.OriginalAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Issuer).HasMaxLength(200);
        builder.Property(x => x.IssuerCnpj).HasMaxLength(20);
        builder.Property(x => x.Status).HasMaxLength(80);
        builder.Property(x => x.FetchedAt).IsRequired();
        builder.Property(x => x.IsCurrent).IsRequired();
        builder.HasIndex(x => new { x.UsuarioId, x.PluggyItemId, x.ExternalInvestmentId })
            .IsUnique().HasDatabaseName("UX_PluggyInvestmentAssetSnapshot_Source");
        builder.HasIndex(x => new { x.UsuarioId, x.ConnectionId, x.IsCurrent })
            .HasDatabaseName("IX_PluggyInvestmentAssetSnapshot_Connection");
        builder.HasOne<PluggyConnection>().WithMany().HasForeignKey(x => x.ConnectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
