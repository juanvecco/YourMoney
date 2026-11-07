using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api;

public static class PluggyWalletIsolationTests
{
    public static Task CredentialsCacheAndProtectionAreNamespacedByWallet()
    {
        var source = File.ReadAllText(RepositoryTestPaths.InYourMoney("src", "YourMoney.Api", "Services", "PluggyService.cs"));
        TestAssert.True(source.Contains("ObterApiKeyAsync(string usuarioId, Guid carteiraId", StringComparison.Ordinal), "API key lookup must require an explicit wallet");
        TestAssert.True(source.Contains("CriarChaveCacheApi(Guid carteiraId, string clientId)", StringComparison.Ordinal), "API key cache should be namespaced by wallet and Client ID");
        TestAssert.True(source.Contains("CreateProtector(\"YourMoney.Pluggy.ClientSecret.v2\", carteiraId.ToString(\"N\"))", StringComparison.Ordinal), "New secret protection should be wallet scoped");
        TestAssert.True(source.Contains("credential.CarteiraId == carteiraId", StringComparison.Ordinal), "Credential queries should include the wallet ID");
        TestAssert.True(source.Contains("CriarConnectTokenPayload(walletId.ToString(\"D\"))", StringComparison.Ordinal), "Provider item callbacks should be tagged with the selected wallet");
        TestAssert.True(source.Contains("connection.CarteiraId == carteiraId", StringComparison.Ordinal), "Provider connection reads should include wallet ownership");
        TestAssert.True(source.Contains("private const int MeuPluggyConnectionLimit = 5", StringComparison.Ordinal), "Each wallet should allow at most five connections");
        TestAssert.True(source.Contains("BeginTransactionAsync(IsolationLevel.Serializable", StringComparison.Ordinal), "Connection-limit checks and inserts should be serialized");
        TestAssert.True(source.Contains("ValidarItemPessoal(usuarioId, carteiraId, item)", StringComparison.Ordinal), "Callbacks must validate the item against the selected owner and wallet");
        TestAssert.True(source.Contains("_dbContext.Carteiras.Any(wallet => wallet.Id == carteiraId && wallet.UsuarioId == usuarioId && wallet.Principal)", StringComparison.Ordinal), "Legacy user-bound items may be accepted only for the principal wallet");
        TestAssert.True(source.Contains("CreateProtector(\"YourMoney.Pluggy.ClientSecret.v1\")", StringComparison.Ordinal), "Legacy v1 secrets must remain readable during migration");
        TestAssert.True(source.Contains("wallet.Id == walletId && wallet.UsuarioId == usuarioId && wallet.Principal", StringComparison.Ordinal), "Server environment credentials must remain limited to the principal wallet");
        TestAssert.True(source.Contains("await ValidarTitularPessoalAsync(usuarioId, carteiraId, cancellationToken)", StringComparison.Ordinal), "Environment credentials must validate both configured titular and default wallet");
        TestAssert.True(source.Contains("_carteiraRepository.ObterOuCriarPrincipalAsync(usuarioId)", StringComparison.Ordinal), "Legacy calls without a wallet must resolve to the existing principal wallet");
        var credentialValidation = source.IndexOf("ValidarCredenciaisInformadasAsync(clientId, request.ClientSecret", StringComparison.Ordinal);
        var credentialPersistence = source.IndexOf("SingleOrDefaultAsync(entry => entry.UsuarioId == usuarioId && entry.CarteiraId == walletId", credentialValidation, StringComparison.Ordinal);
        TestAssert.True(credentialValidation >= 0 && credentialPersistence > credentialValidation, "New credentials must be validated before replacing the stored wallet credential");
        return Task.CompletedTask;
    }
}
