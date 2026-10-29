using Microsoft.EntityFrameworkCore;
using YourMoney.Domain.Entities;
using YourMoney.Infrastructure.Persistence;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Infrastructure;

public static class PluggyTransactionImportConfigurationTests
{
    public static Task SourceAndExpenseTargetsAreUniquePerOwner()
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
        TestAssert.True(targetIndex.IsUnique, "One owner/expense record must resolve to one source transaction");
        return Task.CompletedTask;
    }
}
