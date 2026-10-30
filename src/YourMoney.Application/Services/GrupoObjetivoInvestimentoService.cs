using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;

namespace YourMoney.Application.Services;

public sealed class GrupoObjetivoInvestimentoService : IGrupoObjetivoInvestimentoService
{
    private readonly IGrupoObjetivoInvestimentoRepository _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly IInvestimentoRepository? _investimentos;
    private readonly IAporteObjetivoInvestimentoRepository? _aportes;
    private readonly IMetaMensalService? _metas;

    public GrupoObjetivoInvestimentoService(
        IGrupoObjetivoInvestimentoRepository repository,
        ICurrentUserService currentUser,
        IInvestimentoRepository? investimentos = null,
        IAporteObjetivoInvestimentoRepository? aportes = null,
        IMetaMensalService? metas = null)
    {
        _repository = repository;
        _currentUser = currentUser;
        _investimentos = investimentos;
        _aportes = aportes;
        _metas = metas;
    }

    public async Task<List<GrupoObjetivoInvestimentoResponse>> ListarAsync(bool incluirArquivados = false)
    {
        var usuarioId = _currentUser.UserId;
        var grupos = await _repository.ListarAsync(usuarioId, incluirArquivados);
        var respostas = new List<GrupoObjetivoInvestimentoResponse>(grupos.Count);
        foreach (var grupo in grupos)
            respostas.Add(Mapear(grupo, await _repository.ContarInvestimentosAsync(grupo.Id, usuarioId)));
        return respostas;
    }

    public Task<List<GrupoObjetivoInvestimentoResponse>> ListarAsync() => ListarAsync(false);

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
        var usuarioId = _currentUser.UserId;
        await ValidarGrupoPaiAsync(request.GrupoPaiId, null, usuarioId);
        var grupo = new GrupoObjetivoInvestimento(
            request.Nome ?? string.Empty,
            usuarioId,
            request.ValorAlvo,
            request.DataAlvo,
            request.Descricao,
            request.ImagemUrl,
            request.GrupoPaiId);
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
        await ValidarGrupoPaiAsync(request.GrupoPaiId, id, usuarioId);
        var versao = LerVersao(request.Versao, "A versão atual do objetivo é obrigatória.");
        grupo.Atualizar(request.Nome ?? string.Empty, request.ValorAlvo, request.DataAlvo, request.Descricao, request.ImagemUrl, request.GrupoPaiId);
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

        if ((await _repository.ListarAsync(_currentUser.UserId, true)).Any(g => g.GrupoPaiId == id))
            throw new GrupoObjetivoConflitoException("grupo_possui_subobjetivos", "Transfira ou exclua os subobjetivos antes de excluir este objetivo.");

        await _repository.ExcluirAsync(new ExcluirGrupoObjetivoCommand(
            id,
            _currentUser.UserId,
            versaoOrigem,
            acao,
            request.GrupoDestinoId,
            versaoDestino));
    }

    public async Task<GrupoObjetivoInvestimentoResponse> ArquivarAsync(Guid id, VersaoGrupoObjetivoRequest request) =>
        await AlterarEstadoAsync(id, request, false);

    public async Task<GrupoObjetivoInvestimentoResponse> ReativarAsync(Guid id, VersaoGrupoObjetivoRequest request) =>
        await AlterarEstadoAsync(id, request, true);

    private async Task<GrupoObjetivoInvestimentoResponse> AlterarEstadoAsync(Guid id, VersaoGrupoObjetivoRequest request, bool ativo)
    {
        if (request == null) throw new ArgumentException("A versão atual do objetivo é obrigatória.");
        var usuario = _currentUser.UserId;
        var grupo = await _repository.ObterAsync(id, usuario) ?? throw new KeyNotFoundException("Objetivo financeiro não encontrado.");
        if (!ativo && (await _repository.ListarAsync(usuario, true)).Any(g => g.GrupoPaiId == id && g.Ativo))
            throw new GrupoObjetivoConflitoException("grupo_possui_subobjetivos", "Reative, exclua ou mova os subobjetivos ativos antes de arquivar este objetivo.");
        var versao = LerVersao(request.Versao, "A versão atual do objetivo é obrigatória.");
        if (ativo) grupo.Reativar(); else grupo.Arquivar();
        await _repository.AtualizarAsync(grupo, versao);
        return Mapear(grupo, await _repository.ContarInvestimentosAsync(id, usuario));
    }

    public async Task<List<GrupoObjetivoInvestimentoResumoResponse>> ObterResumoAsync(int mes, int ano, bool incluirArquivados = false)
    {
        if (mes is < 1 or > 12 || ano is < 1900 or > 9999) throw new ArgumentException("Mês ou ano inválido.");
        if (_investimentos == null || _aportes == null || _metas == null) throw new InvalidOperationException("Resumo integrado não configurado.");
        var usuario = _currentUser.UserId;
        var grupos = await _repository.ListarAsync(usuario, incluirArquivados);
        var investimentos = await _investimentos.GetAllAsync(usuario);
        var aportesMes = await _aportes.ListarPorMesAsync(mes, ano, usuario);
        var metasResumo = await _metas.ObterResumoAsync(mes, ano);
        var resultado = new List<GrupoObjetivoInvestimentoResumoResponse>();
        foreach (var grupo in grupos)
        {
            var itens = investimentos.Where(i => i.Ativo && i.GrupoObjetivoInvestimentoId == grupo.Id).ToList();
            var aportes = aportesMes.Where(a => a.GrupoObjetivoInvestimentoId == grupo.Id).ToList();
            var planejado = metasResumo.Metas.Where(m => m.GrupoObjetivoInvestimentoId == grupo.Id).Sum(m => m.ValorCalculado);
            var confirmado = aportes.Sum(a => a.Valor);
            var atual = itens.Sum(i => i.ValorAtual);
            var diferenca = confirmado - planejado;
            var baseDto = Mapear(grupo, itens.Count);
            resultado.Add(new GrupoObjetivoInvestimentoResumoResponse
            {
                Id = baseDto.Id, Nome = baseDto.Nome, Descricao = baseDto.Descricao, ImagemUrl = baseDto.ImagemUrl,
                Versao = baseDto.Versao, Ativo = baseDto.Ativo,
                ArquivadoEm = baseDto.ArquivadoEm, ValorAlvo = baseDto.ValorAlvo, DataAlvo = baseDto.DataAlvo, GrupoPaiId = baseDto.GrupoPaiId,
                QuantidadeInvestimentos = itens.Count, ValorAtualCarteira = atual,
                PercentualAlvoAtingido = grupo.ValorAlvo > 0 ? decimal.Round(atual / grupo.ValorAlvo.Value * 100m, 2) : null,
                ValorPlanejadoMes = planejado, ValorConfirmadoMes = confirmado, DiferencaMes = diferenca,
                StatusPlanejamento = diferenca < 0 ? "AbaixoPlanejado" : diferenca > 0 ? "AcimaPlanejado" : "PlanejadoAtingido",
                ValorAguardandoAlocacao = aportes.Where(a => !a.InvestimentoId.HasValue).Sum(a => a.Valor),
                Investimentos = itens.Select(i => new InvestimentoObjetivoResumoDTO { Id = i.Id, Nome = i.Nome, ValorAtual = i.ValorAtual }).ToList(),
                Aportes = aportes.Select(a => new AporteObjetivoInvestimentoDTO { Id = a.Id, MetaMensalId = a.MetaMensalId,
                    GrupoObjetivoInvestimentoId = a.GrupoObjetivoInvestimentoId, InvestimentoId = a.InvestimentoId,
                    InvestimentoNome = itens.FirstOrDefault(i => i.Id == a.InvestimentoId)?.Nome, Valor = a.Valor,
                    MesReferencia = a.MesReferencia, DataConfirmacao = a.DataConfirmacao }).ToList()
            });
        }

        foreach (var resumo in resultado.Where(r => !r.GrupoPaiId.HasValue))
        {
            var filhos = resultado.Where(r => r.GrupoPaiId == resumo.Id).ToList();
            if (filhos.Count == 0) continue;
            resumo.ValorAtualCarteira += filhos.Sum(f => f.ValorAtualCarteira);
            resumo.QuantidadeInvestimentos += filhos.Sum(f => f.QuantidadeInvestimentos);
            resumo.ValorPlanejadoMes += filhos.Sum(f => f.ValorPlanejadoMes);
            resumo.ValorConfirmadoMes += filhos.Sum(f => f.ValorConfirmadoMes);
            resumo.DiferencaMes = resumo.ValorConfirmadoMes - resumo.ValorPlanejadoMes;
            resumo.StatusPlanejamento = resumo.DiferencaMes < 0 ? "AbaixoPlanejado" : resumo.DiferencaMes > 0 ? "AcimaPlanejado" : "PlanejadoAtingido";
            resumo.ValorAguardandoAlocacao += filhos.Sum(f => f.ValorAguardandoAlocacao);
            resumo.PercentualAlvoAtingido = resumo.ValorAlvo > 0 ? decimal.Round(resumo.ValorAtualCarteira / resumo.ValorAlvo.Value * 100m, 2) : null;
        }
        return resultado;
    }

    private async Task ValidarGrupoPaiAsync(Guid? grupoPaiId, Guid? grupoIdAtual, string usuarioId)
    {
        if (!grupoPaiId.HasValue) return;
        if (grupoPaiId == grupoIdAtual)
            throw new ArgumentException("Um objetivo não pode ser subobjetivo dele mesmo.");

        var grupos = await _repository.ListarAsync(usuarioId, true);
        var atual = grupoIdAtual.HasValue ? grupos.FirstOrDefault(g => g.Id == grupoIdAtual.Value) : null;
        var pai = grupos.FirstOrDefault(g => g.Id == grupoPaiId.Value);
        if (pai == null || !pai.Ativo)
            throw new ArgumentException("Selecione um objetivo principal ativo da sua conta.");
        if (pai.GrupoPaiId.HasValue)
            throw new ArgumentException("Subobjetivos só podem ter um nível de profundidade.");
        if (atual != null && grupos.Any(g => g.GrupoPaiId == atual.Id))
            throw new ArgumentException("Mova os subobjetivos antes de tornar este objetivo um subobjetivo.");
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
        Descricao = grupo.Descricao,
        ImagemUrl = grupo.ImagemUrl,
        Versao = Convert.ToBase64String(grupo.Versao),
        QuantidadeInvestimentos = quantidade
        ,ValorAlvo = grupo.ValorAlvo
        ,DataAlvo = grupo.DataAlvo
        ,Ativo = grupo.Ativo
        ,ArquivadoEm = grupo.ArquivadoEm
        ,GrupoPaiId = grupo.GrupoPaiId
    };
}
