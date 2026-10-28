using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;

namespace YourMoney.Application.Services;

public sealed class ConflitoAporteObjetivoException : InvalidOperationException
{
    public ConflitoAporteObjetivoException(string codigo, string message) : base(message) => Codigo = codigo;
    public string Codigo { get; }
}

public sealed class AporteObjetivoInvestimentoService : IAporteObjetivoInvestimentoService
{
    private readonly IAporteObjetivoInvestimentoRepository _repository;
    private readonly IMetaMensalRepository _metas;
    private readonly IGrupoObjetivoInvestimentoRepository _grupos;
    private readonly IInvestimentoRepository _investimentos;
    private readonly ICurrentUserService _currentUser;

    public AporteObjetivoInvestimentoService(IAporteObjetivoInvestimentoRepository repository,
        IMetaMensalRepository metas, IGrupoObjetivoInvestimentoRepository grupos,
        IInvestimentoRepository investimentos, ICurrentUserService currentUser)
    { _repository = repository; _metas = metas; _grupos = grupos; _investimentos = investimentos; _currentUser = currentUser; }

    public async Task<List<AporteObjetivoInvestimentoDTO>> ListarAsync(Guid metaId)
    {
        var usuario = _currentUser.UserId;
        _ = await _metas.GetByIdAsync(metaId, usuario) ?? throw new KeyNotFoundException("Meta não encontrada.");
        var investimentos = (await _investimentos.GetAllAsync(usuario)).ToDictionary(i => i.Id, i => i.Nome);
        return (await _repository.ListarPorMetaAsync(metaId, usuario)).Select(a => Mapear(a, investimentos, false)).ToList();
    }

    public async Task<AporteObjetivoInvestimentoDTO> ConfirmarAsync(Guid metaId, ConfirmarAporteObjetivoRequest request)
    {
        if (request == null || metaId == Guid.Empty || request.OperacaoId == Guid.Empty)
            throw new ArgumentException("Meta, operação e dados do aporte são obrigatórios.");
        var usuario = _currentUser.UserId;
        var meta = await _metas.GetByIdAsync(metaId, usuario) ?? throw new KeyNotFoundException("Meta não encontrada.");
        if (!meta.GrupoObjetivoInvestimentoId.HasValue)
            throw new ConflitoAporteObjetivoException("meta_sem_objetivo", "Associe a meta a um objetivo antes de confirmar o aporte.");
        var grupo = await _grupos.ObterAsync(meta.GrupoObjetivoInvestimentoId.Value, usuario, false)
            ?? throw new KeyNotFoundException("Objetivo financeiro não encontrado.");
        if (!grupo.Ativo) throw new ConflitoAporteObjetivoException("objetivo_arquivado", "O objetivo está arquivado.");

        var destino = await ValidarDestinoAsync(request.InvestimentoId, grupo.Id, usuario);
        var existente = await _repository.ObterPorOperacaoAsync(request.OperacaoId, usuario);
        if (existente != null)
        {
            if (!existente.Corresponde(meta.Id, grupo.Id, request.Valor, meta.MesReferencia, request.DataConfirmacao, request.InvestimentoId))
                throw new ConflitoAporteObjetivoException("operacao_divergente", "O identificador da operação já foi usado com outros dados.");
            return Mapear(existente, destino == null ? null : new Dictionary<Guid, string> { [destino.Id] = destino.Nome }, false);
        }

        var aporte = new AporteObjetivoInvestimento(usuario, meta.Id, grupo.Id, request.Valor,
            meta.MesReferencia, request.DataConfirmacao, request.OperacaoId, request.InvestimentoId);
        try { await _repository.AdicionarAsync(aporte); }
        catch (InvalidOperationException)
        {
            existente = await _repository.ObterPorOperacaoAsync(request.OperacaoId, usuario);
            if (existente != null && existente.Corresponde(meta.Id, grupo.Id, request.Valor, meta.MesReferencia, request.DataConfirmacao, request.InvestimentoId))
                return Mapear(existente, destino == null ? null : new Dictionary<Guid, string> { [destino.Id] = destino.Nome }, false);
            throw new ConflitoAporteObjetivoException("operacao_divergente", "O identificador da operação já foi usado com outros dados.");
        }
        return Mapear(aporte, destino == null ? null : new Dictionary<Guid, string> { [destino.Id] = destino.Nome }, true);
    }

    public async Task<AporteObjetivoInvestimentoDTO> AlocarAsync(Guid metaId, Guid aporteId, AlocarAporteObjetivoRequest request)
    {
        if (request == null || request.InvestimentoId == Guid.Empty) throw new ArgumentException("Investimento é obrigatório.");
        var usuario = _currentUser.UserId;
        var aporte = await _repository.ObterAsync(aporteId, usuario) ?? throw new KeyNotFoundException("Aporte não encontrado.");
        if (aporte.MetaMensalId != metaId) throw new KeyNotFoundException("Aporte não encontrado para esta meta.");
        var destino = await ValidarDestinoAsync(request.InvestimentoId, aporte.GrupoObjetivoInvestimentoId, usuario)
            ?? throw new KeyNotFoundException("Investimento não encontrado.");
        try { aporte.Alocar(destino.Id); }
        catch (InvalidOperationException ex) { throw new ConflitoAporteObjetivoException("alocacao_imutavel", ex.Message); }
        await _repository.AtualizarAsync(aporte);
        return Mapear(aporte, new Dictionary<Guid, string> { [destino.Id] = destino.Nome }, false);
    }

    private async Task<Investimento?> ValidarDestinoAsync(Guid? id, Guid grupoId, string usuario)
    {
        if (!id.HasValue) return null;
        var investimento = await _investimentos.GetByIdAsync(id.Value, usuario);
        if (investimento == null) throw new KeyNotFoundException("Investimento de destino não encontrado.");
        if (!investimento.Ativo || investimento.GrupoObjetivoInvestimentoId != grupoId)
            throw new ConflitoAporteObjetivoException("destino_invalido", "O investimento deve estar ativo e associado ao mesmo objetivo.");
        return investimento;
    }

    private static AporteObjetivoInvestimentoDTO Mapear(AporteObjetivoInvestimento a, IReadOnlyDictionary<Guid, string>? nomes, bool criado) => new()
    {
        Id = a.Id, MetaMensalId = a.MetaMensalId, GrupoObjetivoInvestimentoId = a.GrupoObjetivoInvestimentoId,
        InvestimentoId = a.InvestimentoId, InvestimentoNome = a.InvestimentoId.HasValue && nomes?.TryGetValue(a.InvestimentoId.Value, out var nome) == true ? nome : null,
        Valor = a.Valor, MesReferencia = a.MesReferencia, DataConfirmacao = a.DataConfirmacao, CriadoAgora = criado
    };
}
