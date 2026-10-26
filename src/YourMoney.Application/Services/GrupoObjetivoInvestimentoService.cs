using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;

namespace YourMoney.Application.Services;

public sealed class GrupoObjetivoInvestimentoService : IGrupoObjetivoInvestimentoService
{
    private readonly IGrupoObjetivoInvestimentoRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GrupoObjetivoInvestimentoService(
        IGrupoObjetivoInvestimentoRepository repository,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<List<GrupoObjetivoInvestimentoResponse>> ListarAsync()
    {
        var usuarioId = _currentUser.UserId;
        var grupos = await _repository.ListarAsync(usuarioId);
        var respostas = new List<GrupoObjetivoInvestimentoResponse>(grupos.Count);
        foreach (var grupo in grupos)
            respostas.Add(Mapear(grupo, await _repository.ContarInvestimentosAsync(grupo.Id, usuarioId)));
        return respostas;
    }

    public async Task<GrupoObjetivoInvestimentoResponse> ObterAsync(Guid id)
    {
        var usuarioId = _currentUser.UserId;
        var grupo = await _repository.ObterAsync(id, usuarioId, false)
            ?? throw new KeyNotFoundException("Objetivo financeiro não encontrado.");
        return Mapear(grupo, await _repository.ContarInvestimentosAsync(id, usuarioId));
    }

    public async Task<GrupoObjetivoInvestimentoResponse> CriarAsync(CriarGrupoObjetivoRequest request)
    {
        if (request == null)
            throw new ArgumentException("Dados do objetivo financeiro são obrigatórios.");
        var grupo = new GrupoObjetivoInvestimento(request.Nome ?? string.Empty, _currentUser.UserId);
        await _repository.AdicionarAsync(grupo);
        return Mapear(grupo, 0);
    }

    public async Task<GrupoObjetivoInvestimentoResponse> RenomearAsync(Guid id, RenomearGrupoObjetivoRequest request)
    {
        if (request == null)
            throw new ArgumentException("Dados do objetivo financeiro são obrigatórios.");
        var usuarioId = _currentUser.UserId;
        var grupo = await _repository.ObterAsync(id, usuarioId)
            ?? throw new KeyNotFoundException("Objetivo financeiro não encontrado.");
        var versao = LerVersao(request.Versao, "A versão atual do objetivo é obrigatória.");
        grupo.Renomear(request.Nome ?? string.Empty);
        await _repository.AtualizarAsync(grupo, versao);
        return Mapear(grupo, await _repository.ContarInvestimentosAsync(id, usuarioId));
    }

    public async Task ExcluirAsync(Guid id, ExcluirGrupoObjetivoRequest request)
    {
        if (request == null)
            throw new ArgumentException("Dados para exclusão são obrigatórios.");
        if (!Enum.TryParse<AcaoVinculosGrupoObjetivo>(request.AcaoVinculos, false, out var acao))
            throw new ArgumentException("Ação de vínculos inválida.");

        var versaoOrigem = LerVersao(request.VersaoOrigem, "A versão atual do objetivo é obrigatória.");
        byte[]? versaoDestino = null;
        if (acao == AcaoVinculosGrupoObjetivo.Transferir)
        {
            if (!request.GrupoDestinoId.HasValue || request.GrupoDestinoId == id)
                throw new ArgumentException("Selecione outro objetivo financeiro como destino.");
            versaoDestino = LerVersao(request.VersaoDestino, "A versão atual do objetivo de destino é obrigatória.");
        }
        else if (request.GrupoDestinoId.HasValue || !string.IsNullOrWhiteSpace(request.VersaoDestino))
        {
            throw new ArgumentException("Destino só pode ser informado ao transferir investimentos.");
        }

        await _repository.ExcluirAsync(new ExcluirGrupoObjetivoCommand(
            id,
            _currentUser.UserId,
            versaoOrigem,
            acao,
            request.GrupoDestinoId,
            versaoDestino));
    }

    private static byte[] LerVersao(string? versao, string mensagem)
    {
        if (string.IsNullOrWhiteSpace(versao))
            throw new ArgumentException(mensagem);
        try { return Convert.FromBase64String(versao); }
        catch (FormatException) { throw new ArgumentException("Versão do objetivo inválida."); }
    }

    private static GrupoObjetivoInvestimentoResponse Mapear(GrupoObjetivoInvestimento grupo, int quantidade) => new()
    {
        Id = grupo.Id,
        Nome = grupo.Nome,
        Versao = Convert.ToBase64String(grupo.Versao),
        QuantidadeInvestimentos = quantidade
    };
}
