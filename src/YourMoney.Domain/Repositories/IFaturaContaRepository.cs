using YourMoney.Domain.Entities;

namespace YourMoney.Domain.Repositories;

public interface IFaturaContaRepository
{
    Task<FaturaConta?> ObterAsync(Guid contaId, DateTime mesReferencia, string usuarioId);
    Task SalvarAsync(FaturaConta fatura);
}
