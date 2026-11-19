using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using YourMoney.Api.Services;
using YourMoney.Infrastructure.Data;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api;

public static class InviteOnboardingTests
{
    public static async Task SingleUseExpiryAndRevocationInPostgreSql()
    {
        var connectionString = Environment.GetEnvironmentVariable("YM_TEST_POSTGRES_IDENTITY")
            ?? throw new InvalidOperationException("YM_TEST_POSTGRES_IDENTITY must point to an isolated test database.");
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(connectionString).Options;
        await using var setup = new ApplicationDbContext(options);
        await setup.Database.EnsureCreatedAsync();
        var adminId = Guid.NewGuid().ToString("N");
        setup.Users.Add(new IdentityUser { Id = adminId, UserName = $"admin-{adminId}@example.test", Email = $"admin-{adminId}@example.test" });
        await setup.SaveChangesAsync();
        var clock = new AdjustableClock(new DateTimeOffset(2026, 11, 19, 12, 0, 0, TimeSpan.Zero));
        var service = new RegistrationInviteService(setup, clock);
        try
        {
        var issued = await service.IssueAsync(adminId, "https://yourmoney-piloto.onrender.com");
        TestAssert.True(issued.Link.Contains("/cadastro?convite="), "Issued invitation has a usable signup link");
        TestAssert.True((await service.CheckAsync(issued.Codigo)).Valido, "New invitation is valid");
        await using var firstDb = new ApplicationDbContext(options);
        await using var secondDb = new ApplicationDbContext(options);
        var first = new RegistrationInviteService(firstDb, clock).ConsumeAsync(issued.Codigo, "user-1");
        var second = new RegistrationInviteService(secondDb, clock).ConsumeAsync(issued.Codigo, "user-2");
        var results = await Task.WhenAll(first, second);
        TestAssert.Equal(1, results.Count(result => result), "Only one concurrent registration can use the link");
        TestAssert.True(!await service.ConsumeAsync(issued.Codigo, "user-2"), "Second use is rejected");
        TestAssert.True(!(await service.CheckAsync(issued.Codigo)).Valido, "Used invitation is no longer valid");
        var listed = await service.ListAsync(adminId);
        TestAssert.Equal(1, listed.Count, "Issuer sees the invitation");
        TestAssert.True(listed[0].UsadoEm.HasValue, "Issuer sees that the invitation was used");

        var revoked = await service.IssueAsync(adminId, "https://yourmoney-piloto.onrender.com");
        var revokeId = (await service.ListAsync(adminId)).Single(item => !item.UsadoEm.HasValue).Id;
        TestAssert.True(await service.RevokeAsync(adminId, revokeId), "Admin can revoke an unused invitation");
        TestAssert.True(!(await service.CheckAsync(revoked.Codigo)).Valido, "Revoked invitation is invalid");

        var expiring = await service.IssueAsync(adminId, "https://yourmoney-piloto.onrender.com");
        clock.Advance(TimeSpan.FromDays(8));
        TestAssert.True(!(await service.CheckAsync(expiring.Codigo)).Valido, "Invitation expires after seven days");
        TestAssert.True(!await service.ConsumeAsync(expiring.Codigo, "user-3"), "Expired invitation cannot be consumed");
        }
        finally
        {
            await setup.UserTokens.Where(token => token.UserId == adminId).ExecuteDeleteAsync();
            await setup.Users.Where(user => user.Id == adminId).ExecuteDeleteAsync();
        }
    }

    private sealed class AdjustableClock(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset _now = now;
        public override DateTimeOffset GetUtcNow() => _now;
        public void Advance(TimeSpan duration) => _now += duration;
    }
}
