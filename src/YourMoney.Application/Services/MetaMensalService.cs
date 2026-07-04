using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;

namespace YourMoney.Application.Services
{
    public class MetaMensalService : IMetaMensalService
    {
        private readonly IMetaMensalRepository _metaMensalRepository;
        private readonly IReceitaRepository _receitaRepository;
        private readonly IDespesaRepository _despesaRepository;
        private readonly ICurrentUserService _currentUserService;

        public MetaMensalService(
            IMetaMensalRepository metaMensalRepository,
            IReceitaRepository receitaRepository,
            IDespesaRepository despesaRepository,
            ICurrentUserService currentUserService)
        {
            _metaMensalRepository = metaMensalRepository;
            _receitaRepository = receitaRepository;
            _despesaRepository = despesaRepository;
            _currentUserService = currentUserService;
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

            var meta = new MetaMensal(request.Nome!, request.PercentualReceita, referencia, usuarioId);
            await _metaMensalRepository.AdicionarAsync(meta);

            var receitaTotal = await _receitaRepository.GetTotalElegivelMetasByMesAnoAsync(
                referencia.Month,
                referencia.Year,
                usuarioId);

            return MapearMeta(meta, receitaTotal);
        }

        public async Task<MetaMensalDTO> AtualizarAsync(Guid id, AtualizarMetaMensalDTO request)
        {
            if (request == null)
                throw new ArgumentException("Dados da meta são obrigatórios.");
            if (id == Guid.Empty || request.Id == Guid.Empty || id != request.Id)
                throw new ArgumentException("Identificador da meta é inválido.");

            ValidarNomePercentual(request.Nome, request.PercentualReceita);

            var usuarioId = _currentUserService.UserId;
            var meta = await _metaMensalRepository.GetByIdAsync(id, usuarioId);
            if (meta == null)
                throw new InvalidOperationException("Meta não encontrada.");

            meta.Atualizar(request.Nome!, request.PercentualReceita);
            await _metaMensalRepository.AtualizarAsync(meta);

            var receitaTotal = await _receitaRepository.GetTotalElegivelMetasByMesAnoAsync(
                meta.MesReferencia.Month,
                meta.MesReferencia.Year,
                usuarioId);

            return MapearMeta(meta, receitaTotal);
        }

        public async Task RemoverAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Identificador da meta é inválido.");

            var usuarioId = _currentUserService.UserId;
            var meta = await _metaMensalRepository.GetByIdAsync(id, usuarioId);
            if (meta == null)
                throw new InvalidOperationException("Meta não encontrada.");

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
            var metasDto = metas.Select(meta => MapearMeta(meta, receitaElegivel)).ToList();
            var percentualTotal = metas.Sum(meta => meta.PercentualReceita);
            var valorReservado = decimal.Round(metasDto.Sum(meta => meta.ValorCalculado), 2, MidpointRounding.AwayFromZero);
            var percentualRestante = 100m - percentualTotal;
            var valorRestanteAntesDespesas = decimal.Round(receitaElegivel - valorReservado, 2, MidpointRounding.AwayFromZero);
            var saldoFinal = decimal.Round(receitaElegivel - valorReservado - despesaTotal, 2, MidpointRounding.AwayFromZero);
            var valorFaltante = saldoFinal < 0 ? Math.Abs(saldoFinal) : 0m;
            var alertas = CriarAlertas(receitaElegivel, percentualTotal, saldoFinal);

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

        private static MetaMensalDTO MapearMeta(MetaMensal meta, decimal receitaTotal)
        {
            return new MetaMensalDTO
            {
                Id = meta.Id,
                Nome = meta.Nome,
                PercentualReceita = meta.PercentualReceita,
                MesReferencia = meta.MesReferencia,
                ValorCalculado = decimal.Round(receitaTotal * meta.PercentualReceita / 100m, 2, MidpointRounding.AwayFromZero)
            };
        }

        private static List<string> CriarAlertas(decimal receitaTotal, decimal percentualTotal, decimal saldoFinal)
        {
            var alertas = new List<string>();

            if (receitaTotal == 0)
                alertas.Add("Ainda não há receita no mês para calcular valores das metas.");
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
            ValidarNomePercentual(request.Nome, request.PercentualReceita);
        }

        private static void ValidarNomePercentual(string? nome, decimal percentualReceita)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome da meta é obrigatório.");
            if (nome.Trim().Length > 100)
                throw new ArgumentException("Nome da meta deve ter no máximo 100 caracteres.");
            if (percentualReceita <= 0)
                throw new ArgumentException("Percentual da receita deve ser maior que zero.");
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
