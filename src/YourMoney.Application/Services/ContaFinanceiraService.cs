using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;

namespace YourMoney.Application.Services;

public sealed class ContaFinanceiraService : IContaFinanceiraService
{
    private readonly IContaFinanceiraRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICarteiraRepository _carteiraRepository;

    public ContaFinanceiraService(
        IContaFinanceiraRepository repository,
        ICurrentUserService currentUser,
        ICarteiraRepository carteiraRepository)
    {
        _repository = repository;
        _currentUserService = currentUser;
        _carteiraRepository = carteiraRepository;
    }

    public async Task<ContaFinanceiraResponse> CriarAsync(ContaFinanceiraRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var walletId = request.CarteiraId.HasValue
            ? (await _carteiraRepository.ObterAsync(request.CarteiraId.Value, _currentUserService.UserId))?.Id
                ?? throw new InvalidOperationException("Carteira não encontrada.")
            : (await _carteiraRepository.ObterOuCriarPrincipalAsync(_currentUserService.UserId)).Id;
        var conta = new ContaFinanceira(request.Descricao, _currentUserService.UserId, walletId);
        await _repository.AdicionarAsync(conta);
        return await MapearAsync(conta);
    }

    public async Task RemoverContaFinanceiraAsync(Guid id)
    {
        var conta = await _repository.GetByIdAsync(id, _currentUserService.UserId);
        if (await _repository.TemVinculosAsync(conta.Id, _currentUserService.UserId))
            throw new ContaFinanceiraEmUsoException();
        await _repository.RemoverAsync(id, _currentUserService.UserId);
    }

    public async Task<ContaFinanceiraResponse> AtualizarAsync(Guid id, ContaFinanceiraRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var conta = await _repository.GetByIdAsync(id, _currentUserService.UserId);
        conta.AtualizarDescricao(request.Descricao);
        await _repository.AtualizarAsync(conta);
        return await MapearAsync(conta);
    }

    public Task<List<ContaFinanceiraResponse>> ListarAsync() => ListarAsync(null);

    public async Task<List<ContaFinanceiraResponse>> ListarAsync(Guid? carteiraId)
    {
        var ownerId = _currentUserService.UserId;
        List<ContaFinanceira> contas;
        if (carteiraId.HasValue)
        {
            if (await _carteiraRepository.ObterAsync(carteiraId.Value, ownerId) is null)
                throw new InvalidOperationException("Carteira não encontrada.");
            contas = await _repository.ListarAsync(ownerId, carteiraId.Value);
        }
        else
        {
            contas = await _repository.ListarAsync(ownerId);
        }
        var responses = new List<ContaFinanceiraResponse>(contas.Count);
        foreach (var conta in contas)
            responses.Add(await MapearAsync(conta));
        return responses;
    }

    public async Task<ContaFinanceiraResponse> GetByIdAsync(Guid id)
    {
        var conta = await _repository.GetByIdAsync(id, _currentUserService.UserId);
        return await MapearAsync(conta);
    }

    public async Task<ContaFinanceiraResponse> MoverAsync(Guid id, MoverContaFinanceiraRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var account = await _repository.GetByIdAsync(id, _currentUserService.UserId);
        if (!request.ConfirmarTransferencia)
            throw new ContaFinanceiraMoverConfirmacaoException();
        if (await _carteiraRepository.ObterAsync(request.CarteiraId, _currentUserService.UserId) is null)
            throw new InvalidOperationException("Carteira não encontrada.");
        account.AssociarCarteira(request.CarteiraId);
        await _repository.AtualizarAsync(account);
        return await MapearAsync(account);
    }

    private async Task<ContaFinanceiraResponse> MapearAsync(ContaFinanceira conta) => new()
    {
        Id = conta.Id,
        Descricao = conta.Descricao,
        DataCriacao = conta.DataCriacao,
        PodeRemover = !await _repository.TemVinculosAsync(conta.Id, _currentUserService.UserId),
        CarteiraId = conta.CarteiraId
    };
}
