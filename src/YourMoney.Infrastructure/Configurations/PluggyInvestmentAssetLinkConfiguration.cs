using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourMoney.Domain.Entities;

namespace YourMoney.Infrastructure.Configurations;

public sealed class PluggyInvestmentAssetLinkConfiguration : IEntityTypeConfiguration<PluggyInvestmentAssetLink>
{
    public void Configure(EntityTypeBuilder<PluggyInvestmentAssetLink> builder)
    {
        builder.ToTable("tbPluggyInvestmentAssetLink");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PluggyItemId).IsRequired().HasMaxLength(36);
        builder.Property(x => x.ExternalInvestmentId).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Action).IsRequired().HasMaxLength(12);
        builder.Property(x => x.RegisteredAt).IsRequired();
        builder.HasIndex(x => new { x.UsuarioId, x.PluggyItemId, x.ExternalInvestmentId })
            .IsUnique().HasDatabaseName("UX_PluggyInvestmentAssetLink_Source");
        builder.HasIndex(x => new { x.UsuarioId, x.InvestimentoId })
            .HasDatabaseName("IX_PluggyInvestmentAssetLink_Investimento");
        builder.HasOne<Investimento>().WithMany()
            .HasForeignKey(x => new { x.InvestimentoId, x.UsuarioId })
            .HasPrincipalKey(x => new { x.Id, x.UsuarioId })
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PluggyConnection>().WithMany().HasForeignKey(x => x.ConnectionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
