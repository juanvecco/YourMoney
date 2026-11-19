using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using YourMoney.Api.Configuration;
using YourMoney.Api.Services;
using YourMoney.Infrastructure.Data;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api;

public static class ExternalOnboardingTests
{
    public static Task MethodsRequireCompleteProviderConfiguration()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ExternalAuth:Google:ClientId"] = "google-client",
            ["ExternalAuth:Apple:ServiceId"] = "apple-service"
        }).Build();
        TestAssert.True(!ExternalProviderConfiguration.GoogleAvailable(config), "Google needs a secret");
        TestAssert.True(!ExternalProviderConfiguration.AppleAvailable(config), "Apple needs team, key and private key");
        config["ExternalAuth:Google:ClientSecret"] = "google-secret";
        TestAssert.True(ExternalProviderConfiguration.GoogleAvailable(config), "Complete Google config enables the method");
        return Task.CompletedTask;
    }

    public static Task AppleSecretUsesServiceIdAndShortExpiry()
    {
        using var key = System.Security.Cryptography.ECDsa.Create(System.Security.Cryptography.ECCurve.NamedCurves.nistP256);
        var settings = new ExternalProviderSettings("service.example", "", "TEAM123", "KEY123",
            Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(key.ExportPkcs8PrivateKeyPem())));
        var encoded = AppleClientSecret.Create(settings);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(encoded);
        TestAssert.Equal("TEAM123", jwt.Issuer, "Apple assertion issuer is team id");
        TestAssert.Equal("service.example", jwt.Subject, "Apple assertion subject is service id");
        TestAssert.True(jwt.ValidTo - jwt.ValidFrom <= TimeSpan.FromMinutes(6), "Apple assertion expires quickly");
        TestAssert.Equal("KEY123", jwt.Header.Kid, "Apple assertion carries key id");
        return Task.CompletedTask;
    }

    public static Task ConfiguredProvidersUseExpectedCallbacks()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["JwtSettings:Secret"] = new string('x', 32),
            ["DATABASE_CONNECTION_STRING"] = "Server=localhost;Database=YourMoneyTest;Trusted_Connection=True;TrustServerCertificate=True",
            ["ExternalAuth:Google:ClientId"] = "google-client",
            ["ExternalAuth:Google:ClientSecret"] = "google-secret",
            ["ExternalAuth:Apple:ServiceId"] = "service.example",
            ["ExternalAuth:Apple:TeamId"] = "TEAM123",
            ["ExternalAuth:Apple:KeyId"] = "KEY123",
            ["ExternalAuth:Apple:PrivateKeyBase64"] = "cHJpdmF0ZS1rZXk="
        }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddIdentityConfiguration(config);
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>();
        var google = options.Get("google");
        var apple = options.Get("apple");
        TestAssert.Equal("/signin-google", google.CallbackPath.Value, "Google uses the registered callback");
        TestAssert.Equal("/signin-apple", apple.CallbackPath.Value, "Apple uses the registered callback");
        TestAssert.Equal(IdentityConstants.ExternalScheme, google.SignInScheme, "Google uses the temporary external cookie");
        TestAssert.Equal(IdentityConstants.ExternalScheme, apple.SignInScheme, "Apple uses the temporary external cookie");
        TestAssert.True(google.UsePkce, "Google authorization uses PKCE");
        TestAssert.Equal("form_post", apple.ResponseMode, "Apple posts authorization results to its callback");
        TestAssert.True(apple.Events.OnAuthorizationCodeReceived != null, "Apple signs the code exchange");
        return Task.CompletedTask;
    }

    public static async Task ProviderSubjectResolvesWithoutEmailAutoLink()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"social-identity-{Guid.NewGuid():N}").Options;
        await using var db = new ApplicationDbContext(options);
        using var store = new UserStore<IdentityUser>(db);
        using var users = new UserManager<IdentityUser>(store, Options.Create(new IdentityOptions()),
            new PasswordHasher<IdentityUser>(), [], [], new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(), null, NullLogger<UserManager<IdentityUser>>.Instance);
        var account = new IdentityUser { UserName = "maria@example.test", Email = "maria@example.test" };
        TestAssert.True((await users.CreateAsync(account)).Succeeded, "Account was created");
        TestAssert.True((await users.AddLoginAsync(account, new UserLoginInfo("google", "stable-provider-sub", "Google"))).Succeeded,
            "Provider subject was linked");

        var sameSubject = await ExternalAccountResolver.ResolveAsync(users, "google", "stable-provider-sub", "changed@example.test", false);
        TestAssert.Equal(ExternalAccountStatus.Linked, sameSubject.Status, "Stable subject reopens its linked account");
        TestAssert.Equal(account.Id, sameSubject.User?.Id, "Linked account does not change with provider email");
        var emailCollision = await ExternalAccountResolver.ResolveAsync(users, "google", "different-sub", "maria@example.test", false);
        TestAssert.Equal(ExternalAccountStatus.EmailAlreadyRegistered, emailCollision.Status,
            "Matching email cannot silently link a different provider identity");
        var loginOnly = await ExternalAccountResolver.ResolveAsync(users, "google", "unknown-sub", "new@example.test", true);
        TestAssert.Equal(ExternalAccountStatus.LoginNotLinked, loginOnly.Status,
            "Login-only flow cannot create a surprise account");
        var newAccount = await ExternalAccountResolver.ResolveAsync(users, "apple", "apple-sub", "new@example.test", false);
        TestAssert.Equal(ExternalAccountStatus.New, newAccount.Status, "Unknown provider subject can begin new registration");
    }
}
