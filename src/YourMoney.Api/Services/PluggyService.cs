using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;
using YourMoney.Infrastructure.Persistence;

namespace YourMoney.Api.Services;

public sealed class PluggyConfigurationException : Exception
{
    public PluggyConfigurationException(string? message = null)
        : base(message ?? "A integração com a Pluggy ainda não foi configurada.")
    {
    }
}

public sealed class PluggyCredentialsInvalidException : Exception
{
    public PluggyCredentialsInvalidException()
        : base("A Pluggy não aceitou essas credenciais. Confira o Client ID e o Client Secret no Dashboard.")
    {
    }
}

public sealed class PluggyCredentialsManagedExternallyException : Exception
{
    public PluggyCredentialsManagedExternallyException()
        : base("As credenciais Pluggy são gerenciadas pela configuração do servidor.")
    {
    }
}

public sealed class PluggyIntegrationException : Exception
{
    public PluggyIntegrationException(string message) : base(message)
    {
    }
}

public sealed class PluggyAuthorizationException : Exception
{
    public PluggyAuthorizationException()
        : base("Esta conta não é o titular configurado para a integração pessoal do Meu Pluggy.")
    {
    }
}

public sealed class PluggyConnectionLimitException : Exception
{
    public PluggyConnectionLimitException()
        : base("O limite gratuito do Meu Pluggy é de cinco conexões ativas.")
    {
    }
}

public sealed class PluggyConsentExpiredException : Exception
{
    public PluggyConsentExpiredException()
        : base("A autorização do Meu Pluggy expirou. Autorize novamente para consultar os dados.")
    {
    }
}

public sealed class PluggyTransactionAlreadyRegisteredException : Exception
{
    public PluggyTransactionImportResultDTO ExistingImport { get; }

    public PluggyTransactionAlreadyRegisteredException(PluggyTransactionImport import)
        : base("Esta transação já foi cadastrada no YourMoney.")
    {
        ExistingImport = new PluggyTransactionImportResultDTO
        {
            TransactionId = import.TransactionId,
            RecordType = import.RecordType,
            RecordId = import.RecordId,
            RegisteredAt = import.RegisteredAt
        };
    }
}

public sealed class PluggyTransactionNotImportableException : Exception
{
    public PluggyTransactionNotImportableException(string message) : base(message)
    {
    }
}

public sealed class PluggyService : IPluggyService
{
    private const string ApiKeyCacheKey = "pluggy-api-key";
    private const int MeuPluggyConnectorId = 200;
    private const int MeuPluggyConnectionLimit = 5;
    private static readonly TimeSpan ApiKeyCacheDuration = TimeSpan.FromMinutes(110);
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PluggyService> _logger;
    private readonly AppDbContext _dbContext;
    private readonly IDataProtector _credentialProtector;
    private readonly IDespesaService _despesaService;
    private readonly IReceitaService _receitaService;
    private static readonly SemaphoreSlim AuthenticationLock = new(1, 1);

    public PluggyService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IMemoryCache cache,
        ILogger<PluggyService> logger,
        AppDbContext dbContext,
        IDataProtectionProvider dataProtectionProvider,
        IDespesaService despesaService,
        IReceitaService receitaService)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _cache = cache;
        _logger = logger;
        _dbContext = dbContext;
        _credentialProtector = dataProtectionProvider.CreateProtector("YourMoney.Pluggy.ClientSecret.v1");
        _despesaService = despesaService;
        _receitaService = receitaService;
    }

    public async Task<PluggyCredentialStatusDTO> ObterConfiguracaoCredenciaisAsync(
        string usuarioId,
        CancellationToken cancellationToken = default)
    {
        ValidarTitularPessoal(usuarioId);
        var environmentCredentials = ObterCredenciaisDoServidor();
        if (environmentCredentials.HasAnyValue)
        {
            return new PluggyCredentialStatusDTO
            {
                Configured = environmentCredentials.IsComplete,
                Source = "server",
                ClientIdHint = CriarDicaClientId(environmentCredentials.ClientId),
                CanSave = false
            };
        }

        var stored = await _dbContext.PluggyCredentials
            .AsNoTracking()
            .SingleOrDefaultAsync(credential => credential.UsuarioId == usuarioId, cancellationToken);
        return new PluggyCredentialStatusDTO
        {
            Configured = stored is not null,
            Source = stored is null ? "none" : "database",
            ClientIdHint = CriarDicaClientId(stored?.ClientId),
            UpdatedAt = stored?.UpdatedAt,
            CanSave = true
        };
    }

    public async Task<PluggyCredentialStatusDTO> SalvarConfiguracaoCredenciaisAsync(
        string usuarioId,
        PluggyCredentialRequestDTO request,
        CancellationToken cancellationToken = default)
    {
        ValidarTitularPessoal(usuarioId);
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.ClientId) || request.ClientId.Length > 200)
            throw new ArgumentException("Informe um Client ID válido.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.ClientSecret) || request.ClientSecret.Length > 1000)
            throw new ArgumentException("Informe um Client Secret válido.", nameof(request));

        if (ObterCredenciaisDoServidor().HasAnyValue)
            throw new PluggyCredentialsManagedExternallyException();

        var clientId = request.ClientId.Trim();
        var apiKey = await ValidarCredenciaisInformadasAsync(clientId, request.ClientSecret, cancellationToken);
        var protectedSecret = _credentialProtector.Protect(request.ClientSecret);
        var credential = await _dbContext.PluggyCredentials
            .SingleOrDefaultAsync(entry => entry.UsuarioId == usuarioId, cancellationToken);
        if (credential is null)
        {
            credential = new PluggyCredential(usuarioId, clientId, protectedSecret);
            _dbContext.PluggyCredentials.Add(credential);
        }
        else
        {
            credential.Atualizar(clientId, protectedSecret);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _cache.Set(ApiKeyCacheKey, apiKey, ApiKeyCacheDuration);
        return new PluggyCredentialStatusDTO
        {
            Configured = true,
            Source = "database",
            ClientIdHint = CriarDicaClientId(clientId),
            UpdatedAt = credential.UpdatedAt,
            CanSave = true
        };
    }

    public async Task<PluggyConnectTokenResponseDTO> CriarConnectTokenAsync(
        string clientUserId,
        Guid? connectionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(clientUserId))
            throw new ArgumentException("Usuário autenticado não identificado.", nameof(clientUserId));

        ValidarTitularPessoal(clientUserId);

        PluggyConnection? connectionToReplace = null;
        if (connectionId.HasValue)
        {
            connectionToReplace = await _dbContext.PluggyConnections
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    entry => entry.Id == connectionId.Value && entry.UsuarioId == clientUserId,
                    cancellationToken);
            if (connectionToReplace is null)
                throw new KeyNotFoundException("Conexão bancária não encontrada.");
        }
        else if (await _dbContext.PluggyConnections.CountAsync(
                     entry => entry.UsuarioId == clientUserId,
                     cancellationToken) >= MeuPluggyConnectionLimit)
        {
            throw new PluggyConnectionLimitException();
        }

        var apiKey = await ObterApiKeyAsync(clientUserId, cancellationToken);
        var requestBody = CriarConnectTokenPayload(clientUserId);
        using var response = await EnviarAsync(HttpMethod.Post, "connect_token", apiKey, requestBody, cancellationToken);
        GarantirSucesso(response, "Não foi possível iniciar a conexão bancária agora. Tente novamente.");

        var body = await response.Content.ReadFromJsonAsync<ConnectTokenResponse>(cancellationToken: cancellationToken);
        if (string.IsNullOrWhiteSpace(body?.AccessToken))
            throw new PluggyIntegrationException("A Pluggy retornou um token inválido.");

        return new PluggyConnectTokenResponseDTO
        {
            AccessToken = body.AccessToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30),
            ConnectionId = connectionToReplace?.Id
        };
    }

    public async Task<PluggyConnectionDTO> RegistrarConexaoAsync(
        string usuarioId,
        string itemId,
        CancellationToken cancellationToken = default)
        => await RegistrarConexaoAsync(usuarioId, itemId, null, cancellationToken);

    public async Task<PluggyConnectionDTO> RegistrarConexaoAsync(
        string usuarioId,
        string itemId,
        Guid? connectionId,
        CancellationToken cancellationToken = default)
    {
        ValidarTitularPessoal(usuarioId);

        if (!Guid.TryParse(itemId, out var parsedItemId))
            throw new ArgumentException("A Pluggy retornou um identificador de conexão inválido.", nameof(itemId));

        var apiKey = await ObterApiKeyAsync(usuarioId, cancellationToken);
        var item = await ObterItemAsync(apiKey, parsedItemId, cancellationToken);
        if (!string.Equals(item.ClientUserId, usuarioId, StringComparison.Ordinal))
            throw new KeyNotFoundException("Conexão bancária não encontrada para este usuário.");
        if (item.Connector?.Id != MeuPluggyConnectorId || item.IsSandbox)
            throw new ArgumentException("Somente conexões reais do conector pessoal Meu Pluggy são aceitas.", nameof(itemId));

        var institutionName = item.Connector?.Name;
        if (string.IsNullOrWhiteSpace(institutionName))
            institutionName = "Meu Pluggy";

        var connectionForItem = await _dbContext.PluggyConnections
            .SingleOrDefaultAsync(entry => entry.PluggyItemId == item.Id, cancellationToken);
        if (connectionForItem is not null && !string.Equals(connectionForItem.UsuarioId, usuarioId, StringComparison.Ordinal))
            throw new KeyNotFoundException("Conexão bancária não encontrada para este usuário.");

        PluggyConnection connection;
        if (connectionId.HasValue)
        {
            connection = await _dbContext.PluggyConnections
                .SingleOrDefaultAsync(
                    entry => entry.Id == connectionId.Value && entry.UsuarioId == usuarioId,
                    cancellationToken)
                ?? throw new KeyNotFoundException("Conexão bancária não encontrada para este usuário.");

            if (connectionForItem is not null && connectionForItem.Id != connection.Id)
                _dbContext.PluggyConnections.Remove(connectionForItem);

            connection.Atualizar(
                item.Id,
                institutionName,
                item.Connector.Id,
                item.Status,
                item.IsSandbox,
                item.LastUpdatedAt,
                item.ConsentExpiresAt);
        }
        else if (connectionForItem is not null)
        {
            connection = connectionForItem;
            connection.Atualizar(
                item.Id,
                institutionName,
                item.Connector.Id,
                item.Status,
                item.IsSandbox,
                item.LastUpdatedAt,
                item.ConsentExpiresAt);
        }
        else
        {
            var count = await _dbContext.PluggyConnections
                .CountAsync(entry => entry.UsuarioId == usuarioId, cancellationToken);
            if (count >= MeuPluggyConnectionLimit)
                throw new PluggyConnectionLimitException();

            connection = new PluggyConnection(
                usuarioId,
                item.Id,
                institutionName,
                item.Connector.Id,
                item.Status,
                item.IsSandbox,
                item.LastUpdatedAt,
                item.ConsentExpiresAt);
            _dbContext.PluggyConnections.Add(connection);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapearConexao(connection);
    }

    public async Task<IReadOnlyList<PluggyConnectionDTO>> ListarConexoesAsync(
        string usuarioId,
        CancellationToken cancellationToken = default)
    {
        ValidarTitularPessoal(usuarioId);
        var connections = await AtualizarMetadadosConexoesAsync(usuarioId, cancellationToken);

        return connections.Select(MapearConexao).ToArray();
    }

    public async Task<IReadOnlyList<PluggyAccountDTO>> ListarContasAsync(
        string usuarioId,
        CancellationToken cancellationToken = default)
    {
        ValidarTitularPessoal(usuarioId);
        var connections = await AtualizarMetadadosConexoesAsync(usuarioId, cancellationToken);
        if (connections.Count == 0)
            return Array.Empty<PluggyAccountDTO>();

        GarantirAutorizacoesAtivas(connections);

        var apiKey = await ObterApiKeyAsync(usuarioId, cancellationToken);
        var accounts = new List<PluggyAccountDTO>();
        foreach (var connection in connections)
        {
            var page = await ObterAsync<PluggyPage<PluggyAccount>>(
                $"accounts?itemId={Uri.EscapeDataString(connection.PluggyItemId)}",
                apiKey,
                "Não foi possível carregar as contas da instituição.",
                cancellationToken);

            foreach (var account in page.Results ?? [])
            {
                accounts.Add(new PluggyAccountDTO
                {
                    ConnectionId = connection.Id,
                    InstitutionName = connection.InstitutionName,
                    AccountId = account.Id,
                    Name = string.IsNullOrWhiteSpace(account.MarketingName) ? account.Name : account.MarketingName,
                    Type = account.Type,
                    Number = MascararNumeroConta(account.Number),
                    Balance = account.Balance,
                    CurrencyCode = string.IsNullOrWhiteSpace(account.CurrencyCode) ? "BRL" : account.CurrencyCode,
                    LastUpdatedAt = connection.LastUpdatedAt
                });
            }
        }

        return accounts;
    }

    public async Task<IReadOnlyList<PluggyTransactionPreviewDTO>> ListarTransacoesPreviewAsync(
        string usuarioId,
        CancellationToken cancellationToken = default)
    {
        ValidarTitularPessoal(usuarioId);
        var accounts = await ListarContasAsync(usuarioId, cancellationToken);
        if (accounts.Count == 0)
            return Array.Empty<PluggyTransactionPreviewDTO>();

        return await ListarTransacoesParaContasAsync(
            usuarioId,
            accounts,
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-30)),
            DateOnly.FromDateTime(DateTime.UtcNow.Date),
            cancellationToken);
    }

    public async Task<PluggyOverviewDTO> ObterVisaoGeralAsync(
        string usuarioId,
        PluggyOverviewFilterDTO filtro,
        CancellationToken cancellationToken = default)
    {
        ValidarTitularPessoal(usuarioId);
        filtro ??= new PluggyOverviewFilterDTO();

        var hoje = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var primeiroDiaDisponivel = hoje.AddDays(-30);
        var inicio = filtro.From.HasValue && filtro.From.Value > primeiroDiaDisponivel
            ? filtro.From.Value
            : primeiroDiaDisponivel;
        var fim = filtro.To.HasValue && filtro.To.Value < hoje ? filtro.To.Value : hoje;
        if (filtro.From.HasValue && filtro.To.HasValue && filtro.From.Value > filtro.To.Value)
            throw new ArgumentException("A data inicial deve ser anterior ou igual à data final.");

        var todasContas = await ListarContasAsync(usuarioId, cancellationToken);
        var contas = todasContas
            .Where(account => !filtro.ConnectionId.HasValue || account.ConnectionId == filtro.ConnectionId.Value)
            .Where(account => string.IsNullOrWhiteSpace(filtro.AccountId)
                || string.Equals(account.AccountId, filtro.AccountId, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var transacoes = contas.Length == 0 || inicio > fim
            ? Array.Empty<PluggyTransactionPreviewDTO>()
            : await ListarTransacoesParaContasAsync(usuarioId, contas, inicio, fim, cancellationToken);

        var saldos = contas
            .Where(account => account.Balance.HasValue)
            .GroupBy(account => new
            {
                CurrencyCode = NormalizarMoeda(account.CurrencyCode),
                AccountType = NormalizarTipoConta(account.Type)
            })
            .Select(group => new PluggyBalanceGroupDTO
            {
                CurrencyCode = group.Key.CurrencyCode,
                AccountType = group.Key.AccountType,
                Balance = decimal.Round(group.Sum(account => account.Balance!.Value), 2, MidpointRounding.AwayFromZero),
                AccountCount = group.Count()
            })
            .OrderBy(group => group.CurrencyCode)
            .ThenBy(group => group.AccountType)
            .ToArray();

        return new PluggyOverviewDTO
        {
            GeneratedAt = DateTimeOffset.UtcNow,
            SourceUpdatedAt = contas.Where(account => account.LastUpdatedAt.HasValue)
                .Select(account => account.LastUpdatedAt)
                .Max(),
            Accounts = contas,
            BalancesByCurrencyAndType = saldos,
            Transactions = transacoes
        };
    }

    private async Task<IReadOnlyList<PluggyTransactionPreviewDTO>> ListarTransacoesParaContasAsync(
        string usuarioId,
        IReadOnlyCollection<PluggyAccountDTO> accounts,
        DateOnly dataInicial,
        DateOnly dataFinal,
        CancellationToken cancellationToken)
    {
        var apiKey = await ObterApiKeyAsync(usuarioId, cancellationToken);
        var dateFrom = dataInicial.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var dateTo = dataFinal.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var transactions = new List<PluggyTransactionPreviewDTO>();

        foreach (var account in accounts)
        {
            var path = "v2/transactions"
                + $"?accountId={Uri.EscapeDataString(account.AccountId)}"
                + $"&dateFrom={dateFrom}&dateTo={dateTo}";
            var page = await ObterAsync<PluggyCursorPage<PluggyTransaction>>(
                path,
                apiKey,
                "Não foi possível carregar as transações da instituição.",
                cancellationToken);

            foreach (var transaction in page.Results ?? [])
            {
                transactions.Add(new PluggyTransactionPreviewDTO
                {
                    TransactionId = transaction.Id,
                    ConnectionId = account.ConnectionId,
                    AccountId = account.AccountId,
                    InstitutionName = account.InstitutionName,
                    AccountName = account.Name,
                    Date = transaction.Date,
                    Description = string.IsNullOrWhiteSpace(transaction.Description)
                        ? "Transação bancária"
                        : transaction.Description,
                    Amount = transaction.Amount,
                    CurrencyCode = string.IsNullOrWhiteSpace(transaction.CurrencyCode) ? "BRL" : transaction.CurrencyCode,
                    AmountInAccountCurrency = transaction.AmountInAccountCurrency,
                    AccountCurrencyCode = string.IsNullOrWhiteSpace(account.CurrencyCode) ? "BRL" : account.CurrencyCode,
                    Direction = NormalizarDirecao(transaction.Type),
                    OperationType = transaction.OperationType,
                    Status = transaction.Status,
                    Importable = ObterValorParaCadastro(transaction.Amount, transaction.CurrencyCode, transaction.AmountInAccountCurrency, account.CurrencyCode).HasValue,
                    ImportUnavailableReason = ObterValorParaCadastro(transaction.Amount, transaction.CurrencyCode, transaction.AmountInAccountCurrency, account.CurrencyCode).HasValue
                        ? null
                        : "A conta ou o valor da transação não possui uma equivalência confiável em reais."
                });
            }
        }

        var latestTransactions = transactions
            .OrderByDescending(transaction => transaction.Date)
            .Take(100)
            .ToArray();
        await AnexarSituacaoCadastroAsync(usuarioId, latestTransactions, cancellationToken);
        return latestTransactions;
    }

    public async Task<PluggyTransactionImportResultDTO> ImportarTransacaoAsync(
        string usuarioId,
        PluggyTransactionImportRequestDTO request,
        CancellationToken cancellationToken = default)
    {
        ValidarTitularPessoal(usuarioId);
        if (request is null)
            throw new ArgumentException("Os dados de cadastro são obrigatórios.");
        if (string.IsNullOrWhiteSpace(request.TransactionId))
            throw new ArgumentException("A transação de origem é obrigatória.");
        if (request.RecordType is not ("expense" or "revenue"))
            throw new ArgumentException("Escolha cadastrar como despesa ou receita.");
        if (string.IsNullOrWhiteSpace(request.Description))
            throw new ArgumentException("A descrição é obrigatória.");
        if (request.Amount <= 0)
            throw new ArgumentException("O valor deve ser maior que zero.");
        if (request.Date == default)
            throw new ArgumentException("A data é obrigatória.");

        var source = (await ListarTransacoesPreviewAsync(usuarioId, cancellationToken))
            .SingleOrDefault(transaction => string.Equals(
                transaction.TransactionId,
                request.TransactionId,
                StringComparison.Ordinal));
        if (source is null)
            throw new KeyNotFoundException("A transação não foi encontrada nas conexões Meu Pluggy deste usuário.");
        if (!source.Importable)
            throw new PluggyTransactionNotImportableException(
                source.ImportUnavailableReason ?? "Esta transação não pode ser cadastrada como valor em reais.");

        var existing = await _dbContext.PluggyTransactionImports
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.UsuarioId == usuarioId && item.TransactionId == source.TransactionId,
                cancellationToken);
        if (existing is not null)
            throw new PluggyTransactionAlreadyRegisteredException(existing);

        var value = decimal.Round(request.Amount, 2, MidpointRounding.AwayFromZero);
        var date = request.Date.ToDateTime(TimeOnly.MinValue);
        var monthReference = new DateTime(date.Year, date.Month, 1);
        await using var transactionScope = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            Guid recordId;
            if (request.RecordType == "expense")
            {
                if (!request.IdContaFinanceira.HasValue || request.IdContaFinanceira.Value == Guid.Empty)
                    throw new ArgumentException("Selecione a conta financeira da despesa.");
                if (!request.IdCategoria.HasValue || request.IdCategoria.Value == Guid.Empty)
                    throw new ArgumentException("Selecione a categoria da despesa.");

                var expense = await _despesaService.CriarDespesaAsync(new CriarDespesaRequest
                {
                    Descricao = request.Description.Trim(),
                    Valor = value,
                    Data = date,
                    MesReferencia = monthReference,
                    IdContaFinanceira = request.IdContaFinanceira.Value,
                    IdCategoria = request.IdCategoria.Value
                });
                recordId = expense.Id;
            }
            else
            {
                if (!request.IdContaFinanceira.HasValue || request.IdContaFinanceira.Value == Guid.Empty)
                    throw new ArgumentException("Selecione a conta financeira da receita.");

                var revenue = await _receitaService.CriarReceitaAsync(new CriarReceitaRequest
                {
                    Descricao = request.Description.Trim(),
                    Valor = value,
                    Data = date,
                    MesReferencia = monthReference,
                    Natureza = request.Natureza,
                    DespesaVinculadaId = request.DespesaVinculadaId,
                    IdContaFinanceira = request.IdContaFinanceira
                });
                recordId = revenue.Id;
            }

            var registeredAt = DateTimeOffset.UtcNow;
            _dbContext.PluggyTransactionImports.Add(new PluggyTransactionImport(
                usuarioId,
                source.TransactionId,
                source.ConnectionId,
                request.RecordType,
                recordId,
                registeredAt));
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transactionScope.CommitAsync(cancellationToken);

            return new PluggyTransactionImportResultDTO
            {
                TransactionId = source.TransactionId,
                RecordType = request.RecordType,
                RecordId = recordId,
                RegisteredAt = registeredAt
            };
        }
        catch (DbUpdateException)
        {
            await transactionScope.RollbackAsync(cancellationToken);
            _dbContext.ChangeTracker.Clear();
            var concurrentImport = await _dbContext.PluggyTransactionImports
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item => item.UsuarioId == usuarioId && item.TransactionId == source.TransactionId,
                    cancellationToken);
            if (concurrentImport is not null)
                throw new PluggyTransactionAlreadyRegisteredException(concurrentImport);
            throw;
        }
        catch
        {
            await transactionScope.RollbackAsync(cancellationToken);
            _dbContext.ChangeTracker.Clear();
            throw;
        }
    }

    public async Task RemoverConexaoAsync(
        string usuarioId,
        Guid connectionId,
        CancellationToken cancellationToken = default)
    {
        ValidarTitularPessoal(usuarioId);
        var connection = await _dbContext.PluggyConnections
            .SingleOrDefaultAsync(
                entry => entry.Id == connectionId && entry.UsuarioId == usuarioId,
                cancellationToken);
        if (connection is null)
            throw new KeyNotFoundException("Conexão bancária não encontrada.");

        _dbContext.PluggyConnections.Remove(connection);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task AnexarSituacaoCadastroAsync(
        string usuarioId,
        IReadOnlyCollection<PluggyTransactionPreviewDTO> transactions,
        CancellationToken cancellationToken)
    {
        if (transactions.Count == 0)
            return;

        var sourceIds = transactions.Select(transaction => transaction.TransactionId).ToArray();
        var registered = await _dbContext.PluggyTransactionImports
            .AsNoTracking()
            .Where(item => item.UsuarioId == usuarioId && sourceIds.Contains(item.TransactionId))
            .ToDictionaryAsync(item => item.TransactionId, cancellationToken);

        var earliest = transactions.Min(transaction => DataTransacaoBrasilia(transaction.Date));
        var latest = transactions.Max(transaction => DataTransacaoBrasilia(transaction.Date));
        var expenses = await _dbContext.Despesas
            .AsNoTracking()
            .Where(expense => expense.UsuarioId == usuarioId
                && expense.Data >= earliest.AddDays(-1)
                && expense.Data <= latest.AddDays(1))
            .Select(expense => new PluggyDuplicateCandidateDTO
            {
                RecordType = "expense",
                RecordId = expense.Id,
                Description = expense.Descricao,
                Date = DateOnly.FromDateTime(expense.Data),
                Amount = expense.Valor
            })
            .ToListAsync(cancellationToken);
        var revenues = await _dbContext.Receitas
            .AsNoTracking()
            .Where(revenue => revenue.UsuarioId == usuarioId
                && revenue.Data >= earliest.AddDays(-1)
                && revenue.Data <= latest.AddDays(1))
            .Select(revenue => new PluggyDuplicateCandidateDTO
            {
                RecordType = "revenue",
                RecordId = revenue.Id,
                Description = revenue.Descricao,
                Date = DateOnly.FromDateTime(revenue.Data),
                Amount = revenue.Valor
            })
            .ToListAsync(cancellationToken);

        foreach (var source in transactions)
        {
            if (registered.TryGetValue(source.TransactionId, out var link))
            {
                source.RegistrationStatus = "REGISTERED";
                source.RegisteredRecordType = link.RecordType;
                source.RegisteredRecordId = link.RecordId;
                source.Importable = false;
                source.ImportUnavailableReason = "Esta transação já está cadastrada no YourMoney.";
                continue;
            }

            var expectedRecordType = source.Direction switch
            {
                "DEBIT" => "expense",
                "CREDIT" => "revenue",
                _ => null
            };
            var amount = ObterValorParaCadastro(
                source.Amount,
                source.CurrencyCode,
                source.AmountInAccountCurrency,
                source.AccountCurrencyCode);
            if (expectedRecordType is null || !amount.HasValue)
                continue;

            var description = NormalizarDescricao(source.Description);
            var sourceDate = DateOnly.FromDateTime(DataTransacaoBrasilia(source.Date));
            var candidates = expectedRecordType == "expense" ? expenses : revenues;
            var candidate = candidates.FirstOrDefault(item =>
                item.Date == sourceDate
                && decimal.Round(item.Amount, 2, MidpointRounding.AwayFromZero) == decimal.Round(amount.Value, 2, MidpointRounding.AwayFromZero)
                && NormalizarDescricao(item.Description) == description);

            if (candidate is not null)
            {
                candidate.Reason = "Mesmo tipo de lançamento, valor, data e descrição após normalização.";
                source.PossibleDuplicate = true;
                source.DuplicateCandidate = candidate;
            }
        }
    }

    private static decimal? ObterValorParaCadastro(
        decimal amount,
        string? transactionCurrency,
        decimal? amountInAccountCurrency,
        string? accountCurrency)
    {
        var accountCode = NormalizarMoeda(accountCurrency);
        var transactionCode = NormalizarMoeda(transactionCurrency);
        if (accountCode != "BRL")
            return null;
        if (transactionCode == "BRL")
            return Math.Abs(amount);
        if (amountInAccountCurrency.HasValue)
            return Math.Abs(amountInAccountCurrency.Value);
        return null;
    }

    private static DateTime DataTransacaoBrasilia(DateTimeOffset date) =>
        date.ToOffset(TimeSpan.FromHours(-3)).Date;

    private static string NormalizarDescricao(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return string.Empty;

        var normalized = description.Normalize(NormalizationForm.FormD);
        var chars = normalized
            .Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            .Where(char.IsLetterOrDigit)
            .Select(char.ToLowerInvariant)
            .ToArray();
        return new string(chars);
    }

    private static string NormalizarMoeda(string? currencyCode) =>
        string.IsNullOrWhiteSpace(currencyCode) ? "UNKNOWN" : currencyCode.Trim().ToUpperInvariant();

    private static string NormalizarTipoConta(string? accountType) =>
        string.Equals(accountType, "CREDIT", StringComparison.OrdinalIgnoreCase)
            ? "CREDIT"
            : string.Equals(accountType, "BANK", StringComparison.OrdinalIgnoreCase)
                ? "BANK"
                : string.IsNullOrWhiteSpace(accountType) ? "UNKNOWN" : accountType.Trim().ToUpperInvariant();

    private static string NormalizarDirecao(string? direction) =>
        string.Equals(direction, "CREDIT", StringComparison.OrdinalIgnoreCase)
            ? "CREDIT"
            : string.Equals(direction, "DEBIT", StringComparison.OrdinalIgnoreCase)
                ? "DEBIT"
                : "UNKNOWN";

    private async Task<List<PluggyConnection>> ListarConexoesDoUsuarioAsync(
        string usuarioId,
        CancellationToken cancellationToken) => await _dbContext.PluggyConnections
            .Where(connection => connection.UsuarioId == usuarioId)
            .OrderBy(connection => connection.ConnectedAt)
            .ToListAsync(cancellationToken);

    private async Task<List<PluggyConnection>> AtualizarMetadadosConexoesAsync(
        string usuarioId,
        CancellationToken cancellationToken)
    {
        var connections = await ListarConexoesDoUsuarioAsync(usuarioId, cancellationToken);
        if (connections.Count == 0)
            return connections;

        var apiKey = await ObterApiKeyAsync(usuarioId, cancellationToken);
        foreach (var connection in connections)
        {
            var itemId = Guid.Parse(connection.PluggyItemId);
            var item = await ObterItemAsync(apiKey, itemId, cancellationToken);
            ValidarItemPessoal(usuarioId, item);
            var status = string.Equals(item.Error?.Code, "USER_AUTHORIZATION_EXPIRED", StringComparison.Ordinal)
                ? item.Error.Code
                : item.Status;
            connection.Atualizar(
                item.Id,
                connection.InstitutionName,
                MeuPluggyConnectorId,
                status,
                item.IsSandbox,
                item.LastUpdatedAt,
                item.ConsentExpiresAt);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return connections;
    }

    private static void GarantirAutorizacoesAtivas(IEnumerable<PluggyConnection> connections)
    {
        var now = DateTimeOffset.UtcNow;
        var currentConnections = connections.ToArray();
        if (currentConnections.Any(connection =>
                (connection.ConsentExpiresAt.HasValue && connection.ConsentExpiresAt.Value <= now)
                || string.Equals(connection.Status, "USER_AUTHORIZATION_EXPIRED", StringComparison.Ordinal)))
            throw new PluggyConsentExpiredException();

        if (currentConnections.Any(connection =>
                !string.Equals(connection.Status, "UPDATED", StringComparison.OrdinalIgnoreCase)))
            throw new PluggyIntegrationException("O Meu Pluggy ainda não disponibilizou dados atualizados para uma das conexões.");
    }

    private async Task<PluggyItem> ObterItemAsync(
        string apiKey,
        Guid itemId,
        CancellationToken cancellationToken) => await ObterAsync<PluggyItem>(
            $"items/{itemId:D}",
            apiKey,
            "Não foi possível validar a conexão com a instituição.",
            cancellationToken);

    private async Task<T> ObterAsync<T>(
        string path,
        string apiKey,
        string publicError,
        CancellationToken cancellationToken)
    {
        using var response = await EnviarAsync(HttpMethod.Get, path, apiKey, null, cancellationToken);
        GarantirSucesso(response, publicError);
        var value = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
        return value ?? throw new PluggyIntegrationException("A Pluggy retornou uma resposta inválida.");
    }

    private async Task<HttpResponseMessage> EnviarAsync(
        HttpMethod method,
        string path,
        string apiKey,
        object? body,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("Pluggy");
        using var request = new HttpRequestMessage(method, path);
        request.Headers.Add("X-API-KEY", apiKey);
        if (body is not null)
            request.Content = JsonContent.Create(body);
        return await client.SendAsync(request, cancellationToken);
    }

    private void GarantirSucesso(
        HttpResponseMessage response,
        string publicError)
    {
        if (response.IsSuccessStatusCode)
            return;

        _logger.LogWarning("A Pluggy respondeu com status {StatusCode}.", (int)response.StatusCode);
        throw new PluggyIntegrationException(publicError);
    }

    private async Task<string> ObterApiKeyAsync(string usuarioId, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue<string>(ApiKeyCacheKey, out var cachedApiKey)
            && !string.IsNullOrWhiteSpace(cachedApiKey))
            return cachedApiKey;

        await AuthenticationLock.WaitAsync(cancellationToken);
        try
        {
            if (_cache.TryGetValue<string>(ApiKeyCacheKey, out cachedApiKey)
                && !string.IsNullOrWhiteSpace(cachedApiKey))
                return cachedApiKey;

            var serverCredentials = ObterCredenciaisDoServidor();
            string? clientId = serverCredentials.ClientId;
            string? clientSecret = serverCredentials.ClientSecret;
            if (serverCredentials.HasAnyValue && !serverCredentials.IsComplete)
                throw new PluggyConfigurationException("A configuração de credenciais Pluggy no servidor está incompleta.");

            if (!serverCredentials.HasAnyValue)
            {
                var storedCredential = await _dbContext.PluggyCredentials
                    .AsNoTracking()
                    .SingleOrDefaultAsync(credential => credential.UsuarioId == usuarioId, cancellationToken);
                if (storedCredential is null)
                    throw new PluggyConfigurationException();

                clientId = storedCredential.ClientId;
                try
                {
                    clientSecret = _credentialProtector.Unprotect(storedCredential.ProtectedClientSecret);
                }
                catch (System.Security.Cryptography.CryptographicException)
                {
                    throw new PluggyConfigurationException(
                        "Não foi possível abrir as credenciais Pluggy protegidas. Verifique se o servidor mantém o mesmo chaveiro de proteção.");
                }
            }

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
                throw new PluggyConfigurationException();

            var apiKey = await AutenticarAplicacaoAsync(clientId, clientSecret, cancellationToken);
            _cache.Set(ApiKeyCacheKey, apiKey, ApiKeyCacheDuration);
            return apiKey;
        }
        finally
        {
            AuthenticationLock.Release();
        }
    }

    private async Task<string> AutenticarAplicacaoAsync(
        string clientId,
        string clientSecret,
        CancellationToken cancellationToken,
        bool validarCredenciaisInformadas = false)
    {
        var client = _httpClientFactory.CreateClient("Pluggy");
        using var response = await client.PostAsJsonAsync(
            "auth",
            new { clientId, clientSecret },
            cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("A Pluggy respondeu à autenticação da aplicação com status {StatusCode}.", (int)response.StatusCode);
            if (validarCredenciaisInformadas
                && (response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                    || response.StatusCode == System.Net.HttpStatusCode.Forbidden))
                throw new PluggyCredentialsInvalidException();
            throw new PluggyIntegrationException("Não foi possível autenticar a integração bancária.");
        }

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken);
        if (string.IsNullOrWhiteSpace(body?.ApiKey))
            throw new PluggyIntegrationException("A Pluggy retornou uma autenticação inválida.");
        return body.ApiKey;
    }

    private async Task<string> ValidarCredenciaisInformadasAsync(
        string clientId,
        string clientSecret,
        CancellationToken cancellationToken) =>
        await AutenticarAplicacaoAsync(clientId, clientSecret, cancellationToken, validarCredenciaisInformadas: true);

    private object CriarConnectTokenPayload(string clientUserId)
    {
        var options = new Dictionary<string, object>
        {
            ["clientUserId"] = clientUserId,
            ["avoidDuplicates"] = true
        };

        var redirectUri = LerConfiguracao("PLUGGY_OAUTH_REDIRECT_URI", "Pluggy:OAuthRedirectUri");
        if (!string.IsNullOrWhiteSpace(redirectUri))
            options["oauthRedirectUri"] = redirectUri;

        var webhookUrl = LerConfiguracao("PLUGGY_WEBHOOK_URL", "Pluggy:WebhookUrl");
        if (!string.IsNullOrWhiteSpace(webhookUrl))
            options["webhookUrl"] = webhookUrl;

        return new Dictionary<string, object> { ["options"] = options };
    }

    private void ValidarTitularPessoal(string usuarioId)
    {
        var titularId = LerConfiguracao("PLUGGY_PERSONAL_OWNER_USER_ID", "Pluggy:PersonalOwnerUserId");
        if (string.IsNullOrWhiteSpace(titularId))
            throw new PluggyConfigurationException();
        if (!string.Equals(titularId.Trim(), usuarioId, StringComparison.Ordinal))
            throw new PluggyAuthorizationException();
    }

    private PluggyCredentialSource ObterCredenciaisDoServidor() => new(
        LerConfiguracao("PLUGGY_CLIENT_ID", "Pluggy:ClientId"),
        LerConfiguracao("PLUGGY_CLIENT_SECRET", "Pluggy:ClientSecret"));

    private static string? CriarDicaClientId(string? clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            return null;
        var normalized = clientId.Trim();
        if (normalized.Length <= 8)
            return new string('•', normalized.Length);
        return $"{normalized[..4]}…{normalized[^4..]}";
    }

    private static void ValidarItemPessoal(string usuarioId, PluggyItem item)
    {
        if (!string.Equals(item.ClientUserId, usuarioId, StringComparison.Ordinal))
            throw new PluggyIntegrationException("A Pluggy não conseguiu validar uma conexão pessoal armazenada.");
        if (item.Connector?.Id != MeuPluggyConnectorId || item.IsSandbox)
            throw new PluggyIntegrationException("A conexão cadastrada não é um item pessoal válido do Meu Pluggy.");
    }

    private static string? MascararNumeroConta(string? number)
    {
        if (string.IsNullOrWhiteSpace(number))
            return null;

        var digits = new string(number.Where(char.IsDigit).ToArray());
        if (digits.Length == 0)
            return null;
        return digits.Length <= 4 ? digits : digits[^4..];
    }

    private string LerConfiguracao(string environmentName, string configurationKey)
    {
        var environmentValue = Environment.GetEnvironmentVariable(environmentName);
        return string.IsNullOrWhiteSpace(environmentValue) ? _configuration[configurationKey] : environmentValue;
    }

    private static PluggyConnectionDTO MapearConexao(PluggyConnection connection) => new()
    {
        Id = connection.Id,
        InstitutionName = connection.InstitutionName,
        Status = connection.Status,
        IsSandbox = connection.IsSandbox,
        LastUpdatedAt = connection.LastUpdatedAt,
        ConsentExpiresAt = connection.ConsentExpiresAt,
        ConnectedAt = connection.ConnectedAt
    };

    private sealed class AuthResponse
    {
        public string ApiKey { get; set; } = string.Empty;
    }

    private readonly record struct PluggyCredentialSource(string? ClientId, string? ClientSecret)
    {
        public bool HasAnyValue => !string.IsNullOrWhiteSpace(ClientId) || !string.IsNullOrWhiteSpace(ClientSecret);
        public bool IsComplete => !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret);
    }

    private sealed class ConnectTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
    }

    private sealed class PluggyPage<T>
    {
        public List<T> Results { get; set; } = [];
    }

    private sealed class PluggyCursorPage<T>
    {
        public List<T> Results { get; set; } = [];
        public string? Next { get; set; }
    }

    private sealed class PluggyItem
    {
        public string Id { get; set; } = string.Empty;
        public int? ConnectorId { get; set; }
        public PluggyConnector? Connector { get; set; }
        public string ClientUserId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsSandbox { get; set; }
        public DateTimeOffset? LastUpdatedAt { get; set; }
        public DateTimeOffset? ConsentExpiresAt { get; set; }
        public PluggyItemError? Error { get; set; }
    }

    private sealed class PluggyItemError
    {
        public string Code { get; set; } = string.Empty;
    }

    private sealed class PluggyConnector
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class PluggyAccount
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? MarketingName { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? Number { get; set; }
        public decimal? Balance { get; set; }
        public string? CurrencyCode { get; set; }
    }

    private sealed class PluggyTransaction
    {
        public string Id { get; set; } = string.Empty;
        public DateTimeOffset Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? CurrencyCode { get; set; }
        public decimal? AmountInAccountCurrency { get; set; }
        public string? Type { get; set; }
        public string? OperationType { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
