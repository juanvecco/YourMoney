using YourMoney.Application.DTOs;

namespace YourMoney.Application.Interfaces;

public interface IConciliacaoFaturaService
{
    Task<ResumoFaturaResponse> ObterAsync(Guid contaId, int mes, int ano);
    Task<ResumoFaturaResponse> SalvarAsync(Guid contaId, int mes, int ano, decimal valorInformado);
}
