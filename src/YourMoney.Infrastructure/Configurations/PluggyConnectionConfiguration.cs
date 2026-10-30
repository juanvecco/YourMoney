using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourMoney.Domain.Entities;

namespace YourMoney.Infrastructure.Configurations;

public sealed class PluggyConnectionConfiguration : IEntityTypeConfiguration<PluggyConnection>
{
    public void Configure(EntityTypeBuilder<PluggyConnection> builder)
    {
        builder.ToTable("tbPluggyConnection");
        builder.HasKey(connection => connection.Id);
        builder.Property(connection => connection.PluggyItemId).IsRequired().HasMaxLength(36);
        builder.Property(connection => connection.InstitutionName).IsRequired().HasMaxLength(160);
        builder.Property(connection => connection.Status).IsRequired().HasMaxLength(40);
        builder.Property(connection => connection.IsSandbox).IsRequired();
        builder.Property(connection => connection.LastUpdatedAt);
        builder.Property(connection => connection.ConsentExpiresAt);
        builder.Property(connection => connection.InvestmentAssetsLastAttemptAt);
        builder.Property(connection => connection.InvestmentAssetsLastSuccessAt);
        builder.Property(connection => connection.InvestmentAssetsSyncStatus)
            .IsRequired().HasMaxLength(24).HasDefaultValue("never");
        builder.Property(connection => connection.InvestmentAssetsSyncMessage).HasMaxLength(500);
        builder.Property(connection => connection.HasInvestmentProduct);
        builder.Property(connection => connection.ConnectedAt).IsRequired();
        builder.HasIndex(connection => connection.PluggyItemId).IsUnique();
    }
}
