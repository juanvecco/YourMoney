using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;

namespace YourMoney.Application.Services;

public sealed class ContaFinanceiraService : IContaFinanceiraService
{
    private readonly IContaFinanceiraRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public ContaFinanceiraService(
        IContaFinanceiraRepository repository,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUserService = currentUser;
    }

    public async Task<ContaFinanceiraResponse> CriarAsync(ContaFinanceiraRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var conta = new ContaFinanceira(request.Descricao, _currentUserService.UserId);
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

    public async Task<List<ContaFinanceiraResponse>> ListarAsync()
    {
        var contas = await _repository.ListarAsync(_currentUserService.UserId);
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

    private async Task<ContaFinanceiraResponse> MapearAsync(ContaFinanceira conta) => new()
    {
        Id = conta.Id,
        Descricao = conta.Descricao,
        DataCriacao = conta.DataCriacao,
        PodeRemover = !await _repository.TemVinculosAsync(conta.Id, _currentUserService.UserId)
    };
}
