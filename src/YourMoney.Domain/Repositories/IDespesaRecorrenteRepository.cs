using YourMoney.Domain.Entities;

namespace YourMoney.Domain.Repositories
{
    public interface IDespesaRecorrenteRepository
    {
        Task AdicionarAsync(DespesaRecorrente recorrencia);
        Task AtualizarAsync(DespesaRecorrente recorrencia);
        Task<DespesaRecorrente?> ObterPorIdAsync(Guid id, string usuarioId);
        Task<List<DespesaRecorrente>> ListarAsync(string usuarioId, bool? ativas = null);
        Task<List<DespesaRecorrente>> ListarElegiveisParaMesAsync(string usuarioId, DateTime mesReferencia);
        Task<DespesaRecorrenteOcorrencia?> ObterOcorrenciaAsync(Guid despesaRecorrenteId, DateTime mesReferencia, string usuarioId);
        Task<DespesaRecorrenteOcorrencia?> ObterOcorrenciaPorIdAsync(Guid ocorrenciaId, string usuarioId);
        Task<List<DespesaRecorrenteOcorrencia>> ListarOcorrenciasPorMesAsync(string usuarioId, DateTime mesReferencia);
        Task AdicionarOcorrenciaAsync(DespesaRecorrenteOcorrencia ocorrencia);
        Task AtualizarOcorrenciaAsync(DespesaRecorrenteOcorrencia ocorrencia);
    }
}
