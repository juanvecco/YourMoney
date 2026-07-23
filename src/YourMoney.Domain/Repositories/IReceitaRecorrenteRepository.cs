using YourMoney.Domain.Entities;

namespace YourMoney.Domain.Repositories
{
    public interface IReceitaRecorrenteRepository
    {
        Task AdicionarAsync(ReceitaRecorrente recorrencia);
        Task AtualizarAsync(ReceitaRecorrente recorrencia);
        Task<ReceitaRecorrente?> ObterPorIdAsync(Guid id, string usuarioId);
        Task<List<ReceitaRecorrente>> ListarAsync(string usuarioId, bool? ativas = null);
        Task<List<ReceitaRecorrente>> ListarElegiveisParaMesAsync(string usuarioId, DateTime mesReferencia);
        Task<List<ReceitaRecorrente>> ListarElegiveisParaReservaAsync(string usuarioId, DateTime mesReferencia);
        Task AdicionarOcorrenciaAsync(ReceitaRecorrenteOcorrencia ocorrencia);
        Task AtualizarOcorrenciaAsync(ReceitaRecorrenteOcorrencia ocorrencia);
        Task<ReceitaRecorrenteOcorrencia?> ObterOcorrenciaAsync(Guid recorrenciaId, DateTime mesReferencia, string usuarioId);
        Task<ReceitaRecorrenteOcorrencia?> ObterOcorrenciaPorIdAsync(Guid ocorrenciaId, string usuarioId);
        Task<List<ReceitaRecorrenteOcorrencia>> ListarOcorrenciasPorMesAsync(string usuarioId, DateTime mesReferencia);
    }
}
