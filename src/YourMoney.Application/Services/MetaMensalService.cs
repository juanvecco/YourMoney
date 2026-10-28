using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;
using YourMoney.Domain.Repositories;

namespace YourMoney.Application.Services
{
    public sealed class ConflitoMetaMensalException : InvalidOperationException
    {
        public ConflitoMetaMensalException(string message) : base(message)
        {
        }
    }

    public class MetaMensalService : IMetaMensalService
    {
        private readonly IMetaMensalRepository _metaMensalRepository;
        private readonly IReceitaRepository _receitaRepository;
        private readonly IDespesaRepository _despesaRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IGrupoObjetivoInvestimentoRepository? _grupoRepository;
        private readonly IAporteObjetivoInvestimentoRepository? _aporteRepository;

        public MetaMensalService(
            IMetaMensalRepository metaMensalRepository,
            IReceitaRepository receitaRepository,
            IDespesaRepository despesaRepository,
            ICurrentUserService currentUserService,
            IGrupoObjetivoInvestimentoRepository? grupoRepository = null,
            IAporteObjetivoInvestimentoRepository? aporteRepository = null)
        {
            _metaMensalRepository = metaMensalRepository;
            _receitaRepository = receitaRepository;
            _despesaRepository = despesaRepository;
            _currentUserService = currentUserService;
            _grupoRepository = grupoRepository;
            _aporteRepository = aporteRepository;
        }

        public async Task<MetasMensaisResumoDTO> ObterResumoAsync(int? mes = null, int? ano = null)
        {
            var referencia = NormalizarReferencia(mes, ano);
            return await MontarResumoAsync(referencia);
        }

        public async Task<MetaMensalDTO> CriarAsync(CriarMetaMensalDTO request)
        {
            ValidarCriacao(request);
            var referencia = NormalizarReferencia(request.MesReferencia);
            var usuarioId = _currentUserService.UserId;
            var tipoDefinicao = ParseTipoDefinicao(request.TipoDefinicao);
            var receitaTotal = await ObterReceitaElegivelAsync(referencia, usuarioId);
            var grupo = await ValidarGrupoAsync(request.GrupoObjetivoInvestimentoId, usuarioId);

            if (tipoDefinicao == TipoDefinicaoMeta.Valor && receitaTotal <= 0)
                throw CriarConflitoSemReceita();

            var meta = new MetaMensal(
                request.Nome!,
                tipoDefinicao,
                request.PercentualReceita,
                request.ValorMeta,
                referencia,
                usuarioId);
            meta.AssociarGrupoObjetivo(request.GrupoObjetivoInvestimentoId);
            await _metaMensalRepository.AdicionarAsync(meta);

            return MapearMeta(meta, receitaTotal, grupo?.Nome);
        }

        public async Task<MetaMensalDTO> AtualizarAsync(Guid id, AtualizarMetaMensalDTO request)
        {
            if (request == null)
                throw new ArgumentException("Dados da meta são obrigatórios.");
            if (id == Guid.Empty || request.Id == Guid.Empty || id != request.Id)
                throw new ArgumentException("Identificador da meta é inválido.");

            ValidarNome(request.Nome);
            var tipoDefinicao = ValidarDefinicao(
                request.TipoDefinicao,
                request.PercentualReceita,
                request.ValorMeta);

            var usuarioId = _currentUserService.UserId;
            var meta = await _metaMensalRepository.GetByIdAsync(id, usuarioId);
            if (meta == null)
                throw new InvalidOperationException("Meta não encontrada.");

            var grupo = await ValidarGrupoAsync(request.GrupoObjetivoInvestimentoId, usuarioId);
            if (meta.GrupoObjetivoInvestimentoId != request.GrupoObjetivoInvestimentoId
                && _aporteRepository != null && await _aporteRepository.ExistePorMetaAsync(id, usuarioId))
                throw new ConflitoMetaMensalException("O objetivo da meta não pode ser alterado após o primeiro aporte.");

            var receitaTotal = await ObterReceitaElegivelAsync(meta.MesReferencia, usuarioId);
            var definicaoAlterada = !meta.PossuiMesmaDefinicao(
                tipoDefinicao,
                request.PercentualReceita,
                request.ValorMeta);

            if (tipoDefinicao == TipoDefinicaoMeta.Valor && definicaoAlterada && receitaTotal <= 0)
                throw CriarConflitoSemReceita();

            if (definicaoAlterada)
            {
                meta.Atualizar(
                    request.Nome!,
                    tipoDefinicao,
                    request.PercentualReceita,
                    request.ValorMeta);
            }
            else
            {
                meta.Renomear(request.Nome!);
            }

            meta.AssociarGrupoObjetivo(request.GrupoObjetivoInvestimentoId);

            await _metaMensalRepository.AtualizarAsync(meta);

            return MapearMeta(meta, receitaTotal, grupo?.Nome);
        }

        public async Task RemoverAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Identificador da meta é inválido.");

            var usuarioId = _currentUserService.UserId;
            var meta = await _metaMensalRepository.GetByIdAsync(id, usuarioId);
            if (meta == null)
                throw new InvalidOperationException("Meta não encontrada.");

            if (_aporteRepository != null && await _aporteRepository.ExistePorMetaAsync(id, usuarioId))
                throw new ConflitoMetaMensalException("A meta possui aportes confirmados e não pode ser excluída.");

            await _metaMensalRepository.RemoverAsync(id, usuarioId);
        }

        private async Task<MetasMensaisResumoDTO> MontarResumoAsync(DateTime referencia)
        {
            var usuarioId = _currentUserService.UserId;
            var receitaElegivel = decimal.Round(
                await _receitaRepository.GetTotalElegivelMetasByMesAnoAsync(referencia.Month, referencia.Year, usuarioId),
                2,
                MidpointRounding.AwayFromZero);
            var receitaBruta = decimal.Round(
                await _receitaRepository.GetTotalBrutoByMesAnoAsync(referencia.Month, referencia.Year, usuarioId),
                2,
                MidpointRounding.AwayFromZero);
            var despesas = await _despesaRepository.ObterPorMesAnoAsync(
                referencia.Month,
                referencia.Year,
                usuarioId,
                null);
            var reembolsos = await _receitaRepository.GetTotaisReembolsadosPorDespesasAsync(
                despesas.Select(d => d.Id).ToList(),
                usuarioId);
            var despesaTotalBruta = decimal.Round(despesas.Sum(despesa => despesa.Valor), 2, MidpointRounding.AwayFromZero);
            var despesaTotalReembolsada = decimal.Round(despesas.Sum(despesa =>
                Math.Min(despesa.Valor, reembolsos.TryGetValue(despesa.Id, out var valor) ? valor : 0m)), 2, MidpointRounding.AwayFromZero);
            var despesaTotal = decimal.Round(despesaTotalBruta - despesaTotalReembolsada, 2, MidpointRounding.AwayFromZero);
            var metas = await _metaMensalRepository.ObterPorMesAnoAsync(referencia.Month, referencia.Year, usuarioId);
            var aportes = _aporteRepository == null
                ? new List<AporteObjetivoInvestimento>()
                : await _aporteRepository.ListarPorMesAsync(referencia.Month, referencia.Year, usuarioId);
            var grupos = _grupoRepository == null
                ? new List<GrupoObjetivoInvestimento>()
                : await _grupoRepository.ListarAsync(usuarioId, true);
            var nomes = grupos.ToDictionary(g => g.Id, g => g.Nome);
            var metasDto = metas.Select(meta => MapearMeta(
                meta,
                receitaElegivel,
                meta.GrupoObjetivoInvestimentoId.HasValue && nomes.TryGetValue(meta.GrupoObjetivoInvestimentoId.Value, out var nome) ? nome : null,
                aportes.Where(a => a.MetaMensalId == meta.Id).ToList())).ToList();
            var existeMetaValorSemBase = receitaElegivel <= 0
                && metas.Any(meta => meta.TipoDefinicao == TipoDefinicaoMeta.Valor);
            decimal? percentualTotal = existeMetaValorSemBase
                ? null
                : decimal.Round(
                    metasDto.Sum(meta => meta.PercentualReceita ?? 0m),
                    4,
                    MidpointRounding.AwayFromZero);
            var valorReservado = decimal.Round(metasDto.Sum(meta => meta.ValorCalculado), 2, MidpointRounding.AwayFromZero);
            decimal? percentualRestante = percentualTotal.HasValue
                ? decimal.Round(100m - percentualTotal.Value, 4, MidpointRounding.AwayFromZero)
                : null;
            var valorRestanteAntesDespesas = decimal.Round(receitaElegivel - valorReservado, 2, MidpointRounding.AwayFromZero);
            var saldoFinal = decimal.Round(receitaElegivel - valorReservado - despesaTotal, 2, MidpointRounding.AwayFromZero);
            var valorFaltante = saldoFinal < 0 ? Math.Abs(saldoFinal) : 0m;
            var alertas = CriarAlertas(receitaElegivel, percentualTotal, saldoFinal, existeMetaValorSemBase);

            return new MetasMensaisResumoDTO
            {
                MesReferencia = referencia,
                ReceitaTotal = receitaElegivel,
                ReceitaTotalBruta = receitaBruta,
                ReceitaElegivelMetas = receitaElegivel,
                ReceitaExcluidaMetas = decimal.Round(receitaBruta - receitaElegivel, 2, MidpointRounding.AwayFromZero),
                DespesaTotal = despesaTotal,
                DespesaTotalBruta = despesaTotalBruta,
                DespesaTotalReembolsada = despesaTotalReembolsada,
                Metas = metasDto,
                PercentualTotalComprometido = percentualTotal,
                ValorTotalReservado = valorReservado,
                PercentualRestante = percentualRestante,
                ValorRestanteAntesDespesas = valorRestanteAntesDespesas,
                SaldoFinal = saldoFinal,
                ValorFaltante = valorFaltante,
                Status = ObterStatus(saldoFinal),
                Alertas = alertas
            };
        }

        private static MetaMensalDTO MapearMeta(MetaMensal meta, decimal receitaTotal, string? objetivoNome = null,
            IReadOnlyCollection<AporteObjetivoInvestimento>? aportes = null)
        {
            decimal valorCalculado;
            decimal? percentualEfetivo;

            if (meta.TipoDefinicao == TipoDefinicaoMeta.Percentual)
            {
                percentualEfetivo = meta.PercentualReceita!.Value;
                valorCalculado = decimal.Round(
                    receitaTotal * percentualEfetivo.Value / 100m,
                    2,
                    MidpointRounding.AwayFromZero);
            }
            else
            {
                valorCalculado = meta.ValorMeta!.Value;
                percentualEfetivo = receitaTotal > 0
                    ? decimal.Round(
                        valorCalculado / receitaTotal * 100m,
                        4,
                        MidpointRounding.AwayFromZero)
                    : null;
            }

            var listaAportes = aportes ?? Array.Empty<AporteObjetivoInvestimento>();
            var confirmado = decimal.Round(listaAportes.Sum(a => a.Valor), 2, MidpointRounding.AwayFromZero);
            var diferenca = decimal.Round(confirmado - valorCalculado, 2, MidpointRounding.AwayFromZero);
            return new MetaMensalDTO
            {
                Id = meta.Id,
                Nome = meta.Nome,
                TipoDefinicao = meta.TipoDefinicao.ToString(),
                PercentualReceita = percentualEfetivo,
                ValorMeta = meta.ValorMeta,
                MesReferencia = meta.MesReferencia,
                ValorCalculado = valorCalculado,
                GrupoObjetivoInvestimentoId = meta.GrupoObjetivoInvestimentoId,
                ObjetivoNome = objetivoNome,
                ValorConfirmado = confirmado,
                ValorRestante = Math.Max(0m, -diferenca),
                Diferenca = diferenca,
                StatusPlanejamento = diferenca < 0 ? "AbaixoPlanejado" : diferenca > 0 ? "AcimaPlanejado" : "PlanejadoAtingido",
                ValorAguardandoAlocacao = listaAportes.Where(a => !a.InvestimentoId.HasValue).Sum(a => a.Valor),
                Aportes = listaAportes.Select(MapearAporte).ToList()
            };
        }

        private static AporteObjetivoInvestimentoDTO MapearAporte(AporteObjetivoInvestimento aporte) => new()
        {
            Id = aporte.Id, MetaMensalId = aporte.MetaMensalId,
            GrupoObjetivoInvestimentoId = aporte.GrupoObjetivoInvestimentoId,
            InvestimentoId = aporte.InvestimentoId, Valor = aporte.Valor,
            MesReferencia = aporte.MesReferencia, DataConfirmacao = aporte.DataConfirmacao
        };

        private async Task<GrupoObjetivoInvestimento?> ValidarGrupoAsync(Guid? grupoId, string usuarioId)
        {
            if (!grupoId.HasValue) return null;
            if (_grupoRepository == null) throw new InvalidOperationException("Repositório de objetivos não configurado.");
            var grupo = await _grupoRepository.ObterAsync(grupoId.Value, usuarioId, false)
                ?? throw new InvalidOperationException("Objetivo financeiro não encontrado.");
            if (!grupo.Ativo) throw new ConflitoMetaMensalException("Objetivos arquivados não podem receber novas metas.");
            return grupo;
        }

        private static List<string> CriarAlertas(
            decimal receitaTotal,
            decimal? percentualTotal,
            decimal saldoFinal,
            bool existeMetaValorSemBase)
        {
            var alertas = new List<string>();

            if (receitaTotal == 0)
                alertas.Add("Ainda não há receita no mês para calcular valores das metas.");
            if (existeMetaValorSemBase)
                alertas.Add("Não há receita elegível positiva para calcular os percentuais das metas por valor.");
            if (percentualTotal > 100m)
                alertas.Add("As metas ultrapassam 100% da receita do mês.");
            if (saldoFinal < 0)
                alertas.Add("Metas e despesas ultrapassam a receita do mês.");

            return alertas;
        }

        private static string ObterStatus(decimal saldoFinal)
        {
            if (saldoFinal > 0) return "disponivel";
            if (saldoFinal < 0) return "faltando";
            return "zerado";
        }

        private static void ValidarCriacao(CriarMetaMensalDTO request)
        {
            if (request == null)
                throw new ArgumentException("Dados da meta são obrigatórios.");
            ValidarNome(request.Nome);
            ValidarDefinicao(request.TipoDefinicao, request.PercentualReceita, request.ValorMeta);
        }

        private static void ValidarNome(string? nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome da meta é obrigatório.");
            if (nome.Trim().Length > 100)
                throw new ArgumentException("Nome da meta deve ter no máximo 100 caracteres.");
        }

        private static TipoDefinicaoMeta ValidarDefinicao(
            string? tipoDefinicao,
            decimal? percentualReceita,
            decimal? valorMeta)
        {
            var tipo = ParseTipoDefinicao(tipoDefinicao);

            if (tipo == TipoDefinicaoMeta.Percentual)
            {
                if (!percentualReceita.HasValue || valorMeta.HasValue)
                    throw new ArgumentException("Meta por percentual deve informar somente o percentual da receita.");
                if (percentualReceita <= 0)
                    throw new ArgumentException("Percentual da receita deve ser maior que zero.");
                if (percentualReceita != decimal.Round(percentualReceita.Value, 4, MidpointRounding.AwayFromZero))
                    throw new ArgumentException("Percentual da receita deve ter no máximo quatro casas decimais.");
            }
            else
            {
                if (!valorMeta.HasValue || percentualReceita.HasValue)
                    throw new ArgumentException("Meta por valor deve informar somente o valor da meta.");
                if (valorMeta <= 0)
                    throw new ArgumentException("Valor da meta deve ser maior que zero.");
                if (valorMeta != decimal.Round(valorMeta.Value, 2, MidpointRounding.AwayFromZero))
                    throw new ArgumentException("Valor da meta deve ter no máximo duas casas decimais.");
            }

            return tipo;
        }

        private static TipoDefinicaoMeta ParseTipoDefinicao(string? tipoDefinicao)
        {
            if (string.IsNullOrWhiteSpace(tipoDefinicao)
                || !Enum.TryParse<TipoDefinicaoMeta>(tipoDefinicao.Trim(), true, out var tipo)
                || !Enum.IsDefined(tipo))
            {
                throw new ArgumentException("Modalidade da meta deve ser Percentual ou Valor.");
            }

            return tipo;
        }

        private async Task<decimal> ObterReceitaElegivelAsync(DateTime referencia, string usuarioId)
        {
            return decimal.Round(
                await _receitaRepository.GetTotalElegivelMetasByMesAnoAsync(
                    referencia.Month,
                    referencia.Year,
                    usuarioId),
                2,
                MidpointRounding.AwayFromZero);
        }

        private static ConflitoMetaMensalException CriarConflitoSemReceita()
        {
            return new ConflitoMetaMensalException(
                "Não é possível definir uma meta por valor sem receita elegível positiva no mês.");
        }

        private static DateTime NormalizarReferencia(DateTime? referencia)
        {
            var valor = referencia ?? DateTime.Today;
            if (valor == default)
                throw new ArgumentException("Mês de referência é obrigatório.");

            return new DateTime(valor.Year, valor.Month, 1);
        }

        private static DateTime NormalizarReferencia(int? mes, int? ano)
        {
            var hoje = DateTime.Today;
            var mesFinal = mes ?? hoje.Month;
            var anoFinal = ano ?? hoje.Year;

            if (mesFinal < 1 || mesFinal > 12)
                throw new ArgumentException("Mês deve estar entre 1 e 12.");
            if (anoFinal < 1900 || anoFinal > 9999)
                throw new ArgumentException("Ano inválido.");

            return new DateTime(anoFinal, mesFinal, 1);
        }
    }
}
