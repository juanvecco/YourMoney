using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourMoney.Domain.Entities;

namespace YourMoney.Infrastructure.Configurations;

public sealed class PluggyAccountLinkConfiguration : IEntityTypeConfiguration<PluggyAccountLink>
{
    public void Configure(EntityTypeBuilder<PluggyAccountLink> builder)
    {
        builder.ToTable("tbPluggyAccountLink");
        builder.HasKey(link => link.Id);
        builder.Property(link => link.AccountId).IsRequired().HasMaxLength(128);
        builder.HasIndex(link => new { link.UsuarioId, link.ConnectionId, link.AccountId }).IsUnique();
        builder.HasOne<PluggyConnection>().WithMany().HasForeignKey(link => link.ConnectionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ContaFinanceira>().WithMany()
            .HasForeignKey(link => new { link.ContaFinanceiraId, link.UsuarioId })
            .HasPrincipalKey(account => new { account.Id, account.UsuarioId })
            .OnDelete(DeleteBehavior.Cascade);
    }
}
