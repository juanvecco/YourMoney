using YourMoney.Domain.Entities;

namespace YourMoney.Domain.Repositories;

public enum AcaoVinculosGrupoObjetivo
{
    ExigirVazio,
    Desassociar,
    Transferir
}

public sealed record ExcluirGrupoObjetivoCommand(
    Guid GrupoOrigemId,
    string UsuarioId,
    byte[] VersaoOrigem,
    AcaoVinculosGrupoObjetivo AcaoVinculos,
    Guid? GrupoDestinoId,
    byte[]? VersaoDestino);

public class GrupoObjetivoConflitoException : InvalidOperationException
{
    public GrupoObjetivoConflitoException(string codigo, string mensagem) : base(mensagem) => Codigo = codigo;
    public string Codigo { get; }
}

public interface IGrupoObjetivoInvestimentoRepository
{
    Task<List<GrupoObjetivoInvestimento>> ListarAsync(string usuarioId);
    Task<List<GrupoObjetivoInvestimento>> ListarAsync(string usuarioId, bool incluirArquivados) => ListarAsync(usuarioId);
    Task<GrupoObjetivoInvestimento?> ObterAsync(Guid id, string usuarioId, bool rastrear = true);
    Task<int> ContarInvestimentosAsync(Guid id, string usuarioId);
    Task<bool> PossuiHistoricoAsync(Guid id, string usuarioId) => Task.FromResult(false);
    Task AdicionarAsync(GrupoObjetivoInvestimento grupo);
    Task AtualizarAsync(GrupoObjetivoInvestimento grupo, byte[] versaoEsperada);
    Task ExcluirAsync(ExcluirGrupoObjetivoCommand command);
}
