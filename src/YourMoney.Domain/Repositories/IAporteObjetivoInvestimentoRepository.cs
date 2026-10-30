using YourMoney.Domain.Entities;

namespace YourMoney.Domain.Repositories;

public interface IAporteObjetivoInvestimentoRepository
{
    Task<AporteObjetivoInvestimento?> ObterPorOperacaoAsync(Guid operacaoId, string usuarioId);
    Task<AporteObjetivoInvestimento?> ObterAsync(Guid id, string usuarioId);
    Task<List<AporteObjetivoInvestimento>> ListarPorMetaAsync(Guid metaId, string usuarioId);
    Task<List<AporteObjetivoInvestimento>> ListarPorMesAsync(int mes, int ano, string usuarioId);
    Task<List<AporteObjetivoInvestimento>> ListarPorObjetivoAsync(Guid objetivoId, string usuarioId);
    Task<bool> ExistePorMetaAsync(Guid metaId, string usuarioId);
    Task AdicionarAsync(AporteObjetivoInvestimento aporte);
    Task AtualizarAsync(AporteObjetivoInvestimento aporte);
}
