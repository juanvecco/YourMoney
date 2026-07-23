using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;
using YourMoney.Domain.Repositories;

namespace YourMoney.Application.Services
{
    public sealed class ConflitoOperacaoInvestimentoException : InvalidOperationException
    {
        public ConflitoOperacaoInvestimentoException()
            : base("O identificador da operação já foi utilizado com dados diferentes.") { }
    }

    public class InvestimentoService : IInvestimentoService
    {
        private readonly IInvestimentoRepository _investimentoRepository;
        private readonly IReceitaRecorrenteRepository _receitaRecorrenteRepository;
        private readonly ICurrentUserService _currentUserService;

        public InvestimentoService(
            IInvestimentoRepository investimentoRepository,
            IReceitaRecorrenteRepository receitaRecorrenteRepository,
            ICurrentUserService currentUserService)
        {
            _investimentoRepository = investimentoRepository;
            _receitaRecorrenteRepository = receitaRecorrenteRepository;
            _currentUserService = currentUserService;
        }

        public async Task AdicionarInvestimentoAsync(Investimento investimento)
        {
            if (investimento.ValorAtual <= 0)
                throw new ArgumentException("O valor do investimento deve ser maior que zero.");

            investimento.DefinirUsuario(_currentUserService.UserId);
            await _investimentoRepository.AdicionarAsync(investimento);
        }

        public async Task<CriarInvestimentoResponse> CriarInvestimentoAsync(CriarInvestimentoRequest request)
        {
            ValidarEscrita(request);
            if (request.OperacaoId == Guid.Empty)
                throw new ArgumentException("Identificador da operação é obrigatório.");

            var usuarioId = _currentUserService.UserId;
            var existente = await _investimentoRepository.ObterPorOperacaoIdAsync(request.OperacaoId, usuarioId);
            if (existente != null)
                return ResolverRepeticao(existente, request);

            await ValidarNovaAssociacaoAsync(request.ReceitaRecorrenteId);
            var investimento = new Investimento(
                request.Nome!,
                request.Descricao!,
                request.Tipo!,
                NormalizarDecimal(request.Quantidade),
                NormalizarDecimal(request.PrecoMedio),
                NormalizarDecimal(request.ValorAtual),
                request.DataInvestimento.Date,
                request.MesReferencia,
                usuarioId,
                request.ReceitaRecorrenteId,
                request.OperacaoId);

            var persistido = await _investimentoRepository.AdicionarIdempotenteAsync(investimento);
            if (persistido.Id != investimento.Id)
                return ResolverRepeticao(persistido, request);

            return MapearCriacao(persistido, true);
        }

        public async Task<InvestimentoResponse> AtualizarInvestimentoAsync(Guid id, AtualizarInvestimentoRequest request)
        {
            ValidarEscrita(request);
            var investimento = await ObterInvestimentoAsync(id);

            if (request.ReceitaRecorrenteId != investimento.ReceitaRecorrenteId)
                await ValidarNovaAssociacaoAsync(request.ReceitaRecorrenteId);

            investimento.AtualizarNome(request.Nome!);
            investimento.AtualizarDescricao(request.Descricao!);
            investimento.AtualizarTipo(request.Tipo!);
            investimento.AtualizarQuantidade(NormalizarDecimal(request.Quantidade));
            investimento.AtualizarPrecoMedio(NormalizarDecimal(request.PrecoMedio));
            investimento.AtualizarValorAtual(NormalizarDecimal(request.ValorAtual));
            investimento.AtualizarData(request.DataInvestimento);
            investimento.AtualizarMesReferencia(request.MesReferencia);
            investimento.DefinirReceitaRecorrente(request.ReceitaRecorrenteId);

            await _investimentoRepository.AtualizarAsync(investimento);
            return Mapear(investimento);
        }

        public Task<Investimento> GetInvestimentoByIdAsync(Guid id) => ObterInvestimentoAsync(id);

        public async Task<InvestimentoResponse> ObterPorIdAsync(Guid id)
        {
            return Mapear(await ObterInvestimentoAsync(id));
        }

        public async Task<CarteiraInvestimentosConsolidadaResponse> ObterConsolidadoAsync()
        {
            var usuarioId = _currentUserService.UserId;
            var mesAtual = ReceitaRecorrente.NormalizarMesReferencia(DateTime.Today);
            var investimentos = await _investimentoRepository.ListarConsolidadoAsync(usuarioId);
            var marcadas = await _receitaRecorrenteRepository.ListarReservasSalariaisAtivasAsync(usuarioId, mesAtual);
            var acumulados = investimentos
                .Where(i => i.ReceitaRecorrenteId.HasValue)
                .GroupBy(i => i.ReceitaRecorrenteId!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.ValorAtual));

            var rendas = investimentos
                .Where(i => i.ReceitaRecorrente != null)
                .Select(i => i.ReceitaRecorrente!)
                .Concat(marcadas)
                .GroupBy(r => r.Id)
                .Select(g => g.First())
                .OrderBy(r => r.Descricao)
                .ThenBy(r => r.Id)
                .ToList();

            return new CarteiraInvestimentosConsolidadaResponse
            {
                TotalInvestido = investimentos.Sum(i => i.ValorAtual),
                Itens = investimentos.Select(Mapear).ToList(),
                Reservas = rendas.Select(r => MapearReserva(
                    r,
                    acumulados.GetValueOrDefault(r.Id),
                    mesAtual)).ToList()
            };
        }

        public async Task RemoverInvestimentoAsync(Guid id)
        {
            _ = await ObterInvestimentoAsync(id);
            await _investimentoRepository.RemoverAsync(id, _currentUserService.UserId);
        }

        public async Task AtualizarAsync(Investimento investimento)
        {
            _ = await ObterInvestimentoAsync(investimento.Id);
            investimento.DefinirUsuario(_currentUserService.UserId);
            await _investimentoRepository.AtualizarAsync(investimento);
        }

        public async Task<List<InvestimentoResponse>> ListarAsync()
        {
            var investimentos = await _investimentoRepository.ListarAsync(_currentUserService.UserId);
            return investimentos.Select(Mapear).ToList();
        }

        public async Task<List<InvestimentoResponse>> ObterPorMesAnoAsync(int mes, int ano)
        {
            var investimentos = await _investimentoRepository.ObterPorMesAnoAsync(mes, ano, _currentUserService.UserId);
            return investimentos.Select(Mapear).ToList();
        }

        private async Task<Investimento> ObterInvestimentoAsync(Guid id)
        {
            var investimento = await _investimentoRepository.GetByIdAsync(id, _currentUserService.UserId);
            return investimento ?? throw new InvalidOperationException("Investimento não encontrado.");
        }

        private async Task ValidarNovaAssociacaoAsync(Guid? receitaRecorrenteId)
        {
            if (!receitaRecorrenteId.HasValue)
                return;

            var receita = await _receitaRecorrenteRepository.ObterPorIdAsync(
                receitaRecorrenteId.Value,
                _currentUserService.UserId);
            if (receita == null || !Elegivel(receita, ReceitaRecorrente.NormalizarMesReferencia(DateTime.Today)))
                throw new ArgumentException("A receita recorrente selecionada não é um salário de renda disponível elegível.");
        }

        private static bool Elegivel(ReceitaRecorrente receita, DateTime mes)
        {
            return receita.EhSalario
                && receita.Natureza == NaturezaReceita.RendaDisponivel
                && receita.EstaElegivelParaMes(mes);
        }

        private static void ValidarEscrita(InvestimentoWriteRequest request)
        {
            if (request == null)
                throw new ArgumentException("Dados do investimento são obrigatórios.");
            if (string.IsNullOrWhiteSpace(request.Nome))
                throw new ArgumentException("Nome do investimento é obrigatório.");
            if (request.Nome.Trim().Length > 100)
                throw new ArgumentException("Nome do investimento deve ter no máximo 100 caracteres.");
            if (string.IsNullOrWhiteSpace(request.Descricao))
                throw new ArgumentException("Descrição do investimento é obrigatória.");
            if (request.Descricao.Trim().Length > 500)
                throw new ArgumentException("Descrição deve ter no máximo 500 caracteres.");
            if (string.IsNullOrWhiteSpace(request.Tipo))
                throw new ArgumentException("Tipo do investimento é obrigatório.");
            if (request.Tipo.Trim().Length > 100)
                throw new ArgumentException("Tipo do investimento deve ter no máximo 100 caracteres.");
            if (request.Quantidade <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero.");
            if (request.PrecoMedio <= 0)
                throw new ArgumentException("Preço médio deve ser maior que zero.");
            if (request.ValorAtual <= 0)
                throw new ArgumentException("Valor atual deve ser maior que zero.");
            if (request.DataInvestimento == default)
                throw new ArgumentException("Data do investimento é obrigatória.");
            if (request.MesReferencia == default)
                throw new ArgumentException("Mês de referência é obrigatório.");
        }

        private static CriarInvestimentoResponse ResolverRepeticao(Investimento existente, CriarInvestimentoRequest request)
        {
            if (!MesmoConteudo(existente, request))
                throw new ConflitoOperacaoInvestimentoException();
            return MapearCriacao(existente, false);
        }

        private static bool MesmoConteudo(Investimento investimento, CriarInvestimentoRequest request)
        {
            var mes = new DateTime(request.MesReferencia.Year, request.MesReferencia.Month, 1);
            return investimento.Nome == request.Nome!.Trim()
                && investimento.Descricao == request.Descricao!.Trim()
                && investimento.Tipo == request.Tipo!.Trim()
                && investimento.Quantidade == NormalizarDecimal(request.Quantidade)
                && investimento.PrecoMedio == NormalizarDecimal(request.PrecoMedio)
                && investimento.ValorAtual == NormalizarDecimal(request.ValorAtual)
                && investimento.DataInvestimento == request.DataInvestimento.Date
                && investimento.MesReferencia == mes
                && investimento.ReceitaRecorrenteId == request.ReceitaRecorrenteId;
        }

        private static decimal NormalizarDecimal(decimal valor) =>
            decimal.Round(valor, 2, MidpointRounding.AwayFromZero);

        private static CriarInvestimentoResponse MapearCriacao(Investimento investimento, bool criadoAgora)
        {
            var response = new CriarInvestimentoResponse { CriadoAgora = criadoAgora };
            PreencherResponse(response, investimento);
            return response;
        }

        private static InvestimentoResponse Mapear(Investimento investimento)
        {
            var response = new InvestimentoResponse();
            PreencherResponse(response, investimento);
            response.MesReferencia ??= new DateTime(investimento.DataInvestimento.Year, investimento.DataInvestimento.Month, 1);
            return response;
        }

        private static void PreencherResponse(InvestimentoResponse response, Investimento investimento)
        {
            response.Id = investimento.Id;
            response.Nome = investimento.Nome;
            response.Descricao = investimento.Descricao;
            response.Tipo = investimento.Tipo;
            response.Quantidade = investimento.Quantidade;
            response.PrecoMedio = investimento.PrecoMedio;
            response.ValorAtual = investimento.ValorAtual;
            response.DataInvestimento = investimento.DataInvestimento;
            response.MesReferencia = investimento.MesReferencia;
            response.DataResgate = investimento.DataResgate;
            response.Ativo = investimento.Ativo;
            response.ReceitaRecorrenteId = investimento.ReceitaRecorrenteId;
            if (investimento.ReceitaRecorrente != null)
            {
                var mes = ReceitaRecorrente.NormalizarMesReferencia(DateTime.Today);
                response.ReservaAssociada = new ReservaAssociadaInvestimentoResponse
                {
                    Descricao = investimento.ReceitaRecorrente.Descricao,
                    ContaDescricao = investimento.ReceitaRecorrente.ContaFinanceira?.Descricao ?? string.Empty,
                    Ativa = investimento.ReceitaRecorrente.Ativa,
                    ElegivelParaNovaAssociacao = Elegivel(investimento.ReceitaRecorrente, mes)
                };
            }
        }

        private static ReservaSalarialResponse MapearReserva(ReceitaRecorrente receita, decimal acumulado, DateTime mes)
        {
            return new ReservaSalarialResponse
            {
                ReceitaRecorrenteId = receita.Id,
                Descricao = receita.Descricao,
                ContaDescricao = receita.ContaFinanceira?.Descricao ?? string.Empty,
                Ativa = receita.Ativa,
                ElegivelParaNovaAssociacao = Elegivel(receita, mes),
                ValorMensal = receita.ValorPrevisto,
                ValorAcumulado = acumulado,
                MetaSeisMeses = CalcularMeta(6, receita.ValorPrevisto, acumulado),
                MetaDozeMeses = CalcularMeta(12, receita.ValorPrevisto, acumulado)
            };
        }

        private static ProgressoMetaReservaResponse CalcularMeta(int meses, decimal mensal, decimal acumulado)
        {
            var meta = mensal * meses;
            return new ProgressoMetaReservaResponse
            {
                Meses = meses,
                ValorMeta = meta,
                ValorRestante = Math.Max(meta - acumulado, 0m),
                PercentualAlcancado = meta == 0m
                    ? 0m
                    : decimal.Round(acumulado / meta * 100m, 2, MidpointRounding.AwayFromZero)
            };
        }
    }
}
