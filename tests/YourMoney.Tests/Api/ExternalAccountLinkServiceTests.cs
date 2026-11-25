using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using YourMoney.Api.Models;
using YourMoney.Api.Services;
using YourMoney.Infrastructure.Data;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api;

public static class ExternalAccountLinkServiceTests
{
    public static Task RequiresVerifiedMatchingEmail()
    {
        var normalizer = new UpperInvariantLookupNormalizer();
        TestAssert.True(ExternalAccountLinkService.IsVerifiedEmailMatch(
            "Maria@Example.com", "maria@example.com", "true", normalizer),
            "Verified Google email matches case-insensitively");
        TestAssert.True(!ExternalAccountLinkService.IsVerifiedEmailMatch(
            "maria@example.com", "maria@example.com", "false", normalizer),
            "An unverified provider email cannot be linked");
        TestAssert.True(!ExternalAccountLinkService.IsVerifiedEmailMatch(
            "maria@example.com", "other@example.com", "true", normalizer),
            "A different provider email cannot be linked");
        TestAssert.True(!ExternalAccountLinkService.IsVerifiedEmailMatch(
            "maria@example.com", null, "true", normalizer),
            "A missing provider email cannot be linked");
        return Task.CompletedTask;
    }

    public static Task MasksProviderEmailForReview()
    {
        TestAssert.Equal("m***@example.com", ExternalAccountLinkService.MaskEmail("maria@example.com"),
            "Review shows only the first local-part character and domain");
        TestAssert.Equal("", ExternalAccountLinkService.MaskEmail("@example.com"),
            "Malformed email is not displayed");
        TestAssert.Equal("", ExternalAccountLinkService.MaskEmail(null),
            "Missing provider email remains empty");
        return Task.CompletedTask;
    }

    public static async Task RevalidatesPasswordWithoutChangingUnconfirmedEmail()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"google-link-password-{Guid.NewGuid():N}").Options;
        await using var db = new ApplicationDbContext(options);
        using var store = new UserStore<IdentityUser>(db);
        using var users = CreateUserManager(store);
        var account = new IdentityUser
        {
            UserName = "maria@example.test",
            Email = "maria@example.test",
            EmailConfirmed = false,
            LockoutEnabled = true
        };
        TestAssert.True((await users.CreateAsync(account, "Senha123!")).Succeeded, "Existing account is available for the link flow");
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var service = new ExternalAccountLinkService(db, users, config);

        var rejected = await service.PrepareAsync(account.Id, "senha-errada");
        TestAssert.True(!rejected.Succeeded && rejected.Failure == ExternalAccountLinkFailure.InvalidPassword,
            "A wrong current password cannot start Google authentication");
        var prepared = await service.PrepareAsync(account.Id, "Senha123!");
        TestAssert.True(prepared.Succeeded && !string.IsNullOrWhiteSpace(prepared.Ticket),
            "The correct password creates a short-lived handoff");
        var token = await db.UserTokens.SingleAsync();
        TestAssert.True(token.Name != prepared.Ticket, "Only the nonce hash is stored in Identity tokens");
        var reloaded = await users.FindByIdAsync(account.Id);
        TestAssert.True(reloaded is { EmailConfirmed: false, Email: "maria@example.test" },
            "Password revalidation must not confirm or change the existing email");
    }

    public static async Task RefusesGoogleIdentityAlreadyOwnedByAnotherAccount()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"google-link-conflict-{Guid.NewGuid():N}").Options;
        await using var db = new ApplicationDbContext(options);
        using var store = new UserStore<IdentityUser>(db);
        using var users = CreateUserManager(store);
        var target = new IdentityUser { UserName = "target@example.test", Email = "target@example.test" };
        var owner = new IdentityUser { UserName = "owner@example.test", Email = "owner@example.test" };
        TestAssert.True((await users.CreateAsync(target)).Succeeded, "Target account exists");
        TestAssert.True((await users.CreateAsync(owner)).Succeeded, "Current Google owner exists");
        TestAssert.True((await users.AddLoginAsync(owner, new UserLoginInfo("google", "stable-google-sub", "Google"))).Succeeded,
            "Google identity belongs to the original account");
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var service = new ExternalAccountLinkService(db, users, config);

        var result = await service.CreatePendingAsync(target.Id, "stable-google-sub", target.Email!);
        TestAssert.True(!result.Succeeded && result.Failure == ExternalAccountLinkFailure.IdentityConflict,
            "A Google identity already owned by another account is refused");
        TestAssert.Equal(owner.Id, (await users.FindByLoginAsync("google", "stable-google-sub"))?.Id,
            "A conflict must not transfer the original provider link");
        TestAssert.Equal(0, (await users.GetLoginsAsync(target)).Count,
            "A conflict must not create a partial link on the authenticated account");
    }

    public static async Task PendingReviewBelongsToCurrentAccountAndMasksEmail()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"google-link-pending-{Guid.NewGuid():N}").Options;
        await using var db = new ApplicationDbContext(options);
        using var store = new UserStore<IdentityUser>(db);
        using var users = CreateUserManager(store);
        var account = new IdentityUser { UserName = "maria@example.test", Email = "maria@example.test" };
        TestAssert.True((await users.CreateAsync(account)).Succeeded, "Current account exists");
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var service = new ExternalAccountLinkService(db, users, config);
        var created = await service.CreatePendingAsync(account.Id, "google-subject", account.Email!);
        TestAssert.True(created.Succeeded && created.Ticket != null, "Verified matching provider can enter review");
        var pendingCookie = new GoogleLinkPendingTicket(account.Id, "google-subject", account.Email!, created.Ticket!);

        var pending = await service.GetPendingAsync(account.Id, pendingCookie);
        TestAssert.Equal("m***@example.test", pending?.EmailMasked,
            "Review returns only a masked address");
        TestAssert.True(pending!.ExpiresAt > DateTimeOffset.UtcNow, "Review is time limited");
        TestAssert.True(await service.GetPendingAsync("another-account", pendingCookie) == null,
            "The pending identity cannot be read by a different account session");
    }

    public static async Task RejectsExpiredPendingAndStartTickets()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"google-link-expired-{Guid.NewGuid():N}").Options;
        await using var db = new ApplicationDbContext(options);
        using var store = new UserStore<IdentityUser>(db);
        using var users = CreateUserManager(store);
        var account = new IdentityUser { UserName = "maria@example.test", Email = "maria@example.test" };
        TestAssert.True((await users.CreateAsync(account)).Succeeded, "Current account exists");
        static string Hash(string nonce) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(nonce)));
        db.UserTokens.AddRange(
            new IdentityUserToken<string>
            {
                UserId = account.Id,
                LoginProvider = "YourMoneyGoogleLinkPending",
                Name = Hash("expired-pending"),
                Value = DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeSeconds().ToString()
            },
            new IdentityUserToken<string>
            {
                UserId = account.Id,
                LoginProvider = "YourMoneyGoogleLinkStart",
                Name = Hash("expired-start"),
                Value = DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeSeconds().ToString()
            });
        await db.SaveChangesAsync();
        var service = new ExternalAccountLinkService(db, users, new ConfigurationBuilder().AddInMemoryCollection().Build());
        var pending = new GoogleLinkPendingTicket(account.Id, "google-subject", account.Email!, "expired-pending");

        TestAssert.True(await service.GetPendingAsync(account.Id, pending) == null,
            "Expired confirmation data is not shown to the person");
        TestAssert.True(!await service.ConsumeStartTicketAsync(account.Id, "expired-start"),
            "Expired OIDC handoffs cannot be consumed");
    }

    private static UserManager<IdentityUser> CreateUserManager(UserStore<IdentityUser> store) =>
        new(store, Options.Create(new IdentityOptions()), new PasswordHasher<IdentityUser>(), [], [],
            new UpperInvariantLookupNormalizer(), new IdentityErrorDescriber(), null,
            NullLogger<UserManager<IdentityUser>>.Instance);
}
