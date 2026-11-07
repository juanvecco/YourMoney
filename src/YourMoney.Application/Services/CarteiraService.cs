using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;

namespace YourMoney.Application.Services;

public sealed class CarteiraService : ICarteiraService
{
    private readonly ICarteiraRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public CarteiraService(ICarteiraRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<CarteiraResponseDTO>> ListarAsync()
    {
        var ownerId = _currentUser.UserId;
        await _repository.ObterOuCriarPrincipalAsync(ownerId);
        return (await _repository.ListarAsync(ownerId)).Select(Mapear).ToList();
    }

    public async Task<CarteiraResponseDTO> CriarAsync(CarteiraRequestDTO request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var carteira = new Carteira(_currentUser.UserId, request.Nome);
        if (await _repository.NomeExisteAsync(carteira.UsuarioId, carteira.NomeNormalizado))
            throw new CarteiraNomeDuplicadoException();

        await _repository.AdicionarAsync(carteira);

        return Mapear(carteira);
    }

    public async Task<CarteiraResponseDTO> RenomearAsync(Guid id, CarteiraRequestDTO request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var carteira = await _repository.ObterAsync(id, _currentUser.UserId)
            ?? throw new CarteiraNaoEncontradaException();
        carteira.Renomear(request.Nome);

        if (await _repository.NomeExisteAsync(carteira.UsuarioId, carteira.NomeNormalizado, carteira.Id))
            throw new CarteiraNomeDuplicadoException();

        await _repository.AtualizarAsync(carteira);

        return Mapear(carteira);
    }

    public async Task RemoverAsync(Guid id)
    {
        var carteira = await _repository.ObterAsync(id, _currentUser.UserId)
            ?? throw new CarteiraNaoEncontradaException();
        if (carteira.Principal)
            throw new CarteiraPrincipalException();
        if (await _repository.PossuiDependenciasAsync(id, _currentUser.UserId))
            throw new CarteiraComDependenciasException();

        await _repository.RemoverAsync(carteira);
    }

    public async Task<Guid> ObterIdPrincipalAsync() =>
        (await _repository.ObterOuCriarPrincipalAsync(_currentUser.UserId)).Id;

    private static CarteiraResponseDTO Mapear(Carteira carteira) => new()
    {
        Id = carteira.Id,
        Nome = carteira.Nome,
        Principal = carteira.Principal
    };
}
