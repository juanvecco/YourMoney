using YourMoney.Application.DTOs;

namespace YourMoney.Application.Interfaces;

public interface IMovimentacaoFinanceiraService
{
    Task<ConsultaMovimentacoesResponse> ConsultarAsync(ConsultaMovimentacoesRequest request);
}
