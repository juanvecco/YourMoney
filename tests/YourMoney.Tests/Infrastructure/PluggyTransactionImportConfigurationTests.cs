using Microsoft.EntityFrameworkCore;
using YourMoney.Domain.Entities;
using YourMoney.Infrastructure.Persistence;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Infrastructure;

public static class PluggyTransactionImportConfigurationTests
{
    public static Task SourceIsUniqueAndInvestmentTargetsCanAggregateSources()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=(localdb);Database=YourMoneyModelOnly;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        using var context = new AppDbContext(options);
        var entity = context.Model.FindEntityType(typeof(PluggyTransactionImport));
        TestAssert.True(entity != null, "Pluggy import mapping should be part of AppDbContext");

        var sourceIndex = entity!.GetIndexes().Single(index =>
            index.Properties.Select(property => property.Name)
                .SequenceEqual(new[] { "UsuarioId", "TransactionId" }));
        var targetIndex = entity.GetIndexes().Single(index =>
            index.Properties.Select(property => property.Name)
                .SequenceEqual(new[] { "UsuarioId", "RecordType", "RecordId" }));

        TestAssert.True(sourceIndex.IsUnique, "One owner/source transaction must resolve to one ledger record");
        TestAssert.True(targetIndex.IsUnique, "Expense and revenue targets must remain unique");
        TestAssert.Equal(
            "[RecordType] <> 'investment'",
            targetIndex.GetFilter()!,
            "An aggregated investment position may be linked to more than one source transaction");
        _ = new PluggyTransactionImport(
            "user-123", "investment-source", Guid.NewGuid(), "investment", Guid.NewGuid(), DateTimeOffset.UtcNow);
        return Task.CompletedTask;
    }
}
