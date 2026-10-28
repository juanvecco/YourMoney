using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourMoney.Domain.Entities;

namespace YourMoney.Infrastructure.Configurations;

public sealed class PluggyTransactionImportConfiguration : IEntityTypeConfiguration<PluggyTransactionImport>
{
    public void Configure(EntityTypeBuilder<PluggyTransactionImport> builder)
    {
        builder.ToTable("tbPluggyTransactionImport");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TransactionId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ConnectionId).IsRequired();
        builder.Property(x => x.RecordType).HasMaxLength(16).IsRequired();
        builder.Property(x => x.RecordId).IsRequired();
        builder.Property(x => x.RegisteredAt).IsRequired();

        builder.HasIndex(x => new { x.UsuarioId, x.TransactionId })
            .IsUnique()
            .HasDatabaseName("UX_PluggyTransactionImport_Usuario_Transaction");

        builder.HasIndex(x => new { x.UsuarioId, x.RecordType, x.RecordId })
            .HasDatabaseName("IX_PluggyTransactionImport_Usuario_Record");
    }
}
