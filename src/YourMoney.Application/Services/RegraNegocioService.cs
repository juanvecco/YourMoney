using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Repositories;

namespace YourMoney.Application.Services;

public sealed class RegraNegocioService : IRegraNegocioService
{
    private readonly IRegraNegocioUsuarioRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public RegraNegocioService(IRegraNegocioUsuarioRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<RegraNegocioDto>> ListarAsync()
    {
        var escolhas = (await _repository.ListarAsync(_currentUser.UserId))
            .ToDictionary(item => item.Codigo, item => item.Ativa);
        var efetivas = new Dictionary<string, bool>();

        bool Efetiva(string codigo)
        {
            if (efetivas.TryGetValue(codigo, out var cached)) return cached;
            var definicao = RegraNegocioCatalogo.Obter(codigo)
                ?? throw new KeyNotFoundException("Regra de negócio não encontrada.");
            var escolha = escolhas.TryGetValue(codigo, out var ativa) ? ativa : definicao.AtivaPorPadrao;
            var result = escolha && definicao.Dependencias.All(Efetiva);
            efetivas[codigo] = result;
            return result;
        }

        return RegraNegocioCatalogo.Todas
            .OrderBy(item => item.Grupo).ThenBy(item => item.Nome)
            .Select(item => new RegraNegocioDto(
                item.Codigo, item.Nome, item.Descricao, item.Grupo,
                escolhas.TryGetValue(item.Codigo, out var ativa) ? ativa : item.AtivaPorPadrao,
                Efetiva(item.Codigo), item.Dependencias))
            .ToArray();
    }

    public async Task<RegraNegocioDto> AtualizarAsync(string codigo, bool ativa)
    {
        if (RegraNegocioCatalogo.Obter(codigo) is null)
            throw new KeyNotFoundException("Regra de negócio não encontrada.");
        await _repository.SalvarAsync(_currentUser.UserId, codigo, ativa);
        return (await ListarAsync()).Single(item => item.Codigo == codigo);
    }

    public async Task<bool> EstaAtivaAsync(string codigo) =>
        (await ListarAsync()).Single(item => item.Codigo == codigo).Efetiva;
}
