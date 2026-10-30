namespace YourMoney.Api.Services;

public interface IUsuarioDemonstracaoService
{
    Task CriarDadosIniciaisAsync(string usuarioId, CancellationToken cancellationToken = default);
}
