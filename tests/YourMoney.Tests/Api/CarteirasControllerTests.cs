using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Api.Controllers;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api;

public static class CarteirasControllerTests
{
    public static async Task EndpointsRequireAuthorizationAndHideForeignWallets()
    {
        var hasAuthorize = typeof(CarteirasController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Any();
        TestAssert.True(hasAuthorize, "Wallet endpoints must require authentication");

        var controller = new CarteirasController(new FakeCarteiraService());
        var result = await controller.Renomear(Guid.NewGuid(), new CarteiraRequestDTO { Nome = "Não existe" });
        TestAssert.Equal(404, ((ObjectResult)result).StatusCode, "Foreign wallets should return the same 404 as missing wallets");
    }

    public static async Task MapsNameAndDependencyConflicts()
    {
        var service = new FakeCarteiraService();
        var controller = new CarteirasController(service);

        service.Error = new CarteiraNomeDuplicadoException();
        var duplicate = await controller.Criar(new CarteiraRequestDTO { Nome = "Repetida" });
        TestAssert.Equal(409, ((ObjectResult)duplicate).StatusCode, "Duplicate wallet names should return conflict");

        service.Error = new CarteiraComDependenciasException();
        var blockedDelete = await controller.Remover(Guid.NewGuid());
        TestAssert.Equal(409, ((ObjectResult)blockedDelete).StatusCode, "Non-empty wallets should not be deleted");
    }

    private sealed class FakeCarteiraService : ICarteiraService
    {
        public Exception? Error { get; set; }
        public Task<IReadOnlyList<CarteiraResponseDTO>> ListarAsync() => Task.FromResult<IReadOnlyList<CarteiraResponseDTO>>([]);
        public Task<CarteiraResponseDTO> CriarAsync(CarteiraRequestDTO request) =>
            Error is null ? Task.FromResult(new CarteiraResponseDTO()) : Task.FromException<CarteiraResponseDTO>(Error);
        public Task<CarteiraResponseDTO> RenomearAsync(Guid id, CarteiraRequestDTO request) =>
            Task.FromException<CarteiraResponseDTO>(new CarteiraNaoEncontradaException());
        public Task RemoverAsync(Guid id) => Error is null ? Task.CompletedTask : Task.FromException(Error);
        public Task<Guid> ObterIdPrincipalAsync() => Task.FromResult(Guid.NewGuid());
    }
}
