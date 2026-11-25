using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Repositories;
using YourMoney.Domain.Enums;

namespace YourMoney.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IReceitaRepository _receitaRepository;
        private readonly IDespesaRepository _despesaRepository;
        private readonly IInvestimentoRepository _investimentoRepository;
        private readonly IMetaRepository _metaRepository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly ICurrentUserService _currentUserService;

        public DashboardService(
            IReceitaRepository receitaRepository,
            IDespesaRepository despesaRepository,
            IInvestimentoRepository investimentoRepository,
            IMetaRepository metaRepository,
            ICategoriaRepository categoriaRepository,
            ICurrentUserService currentUserService)
        {
            _receitaRepository = receitaRepository;
            _despesaRepository = despesaRepository;
            _investimentoRepository = investimentoRepository;
            _metaRepository = metaRepository;
            _categoriaRepository = categoriaRepository;
            _currentUserService = currentUserService;
        }

        public async Task<DashboardDTO> GetDashboardDataAsync(int mes, int ano)
        {
            var usuarioId = _currentUserService.UserId;
            var receitas = await _receitaRepository.GetByMesAnoAsync(mes, ano, usuarioId);
            var despesas = await _despesaRepository.GetByMesAnoAsync(mes, ano, usuarioId);
            var investimentos = await _investimentoRepository.GetAllAsync(usuarioId);
            var metasAtivas = await _metaRepository.GetAtivasAsync(usuarioId);

            var totalReceitas = receitas.Sum(r => r.Valor);
            var totalDespesas = despesas.Sum(d => d.Valor);
            //var totalInvestimentos = investimentos.Sum(i => i.ValorInvestido);

            return new DashboardDTO
            {
                TotalReceitas = totalReceitas,
                TotalDespesas = totalDespesas,
                //TotalInvestimentos = totalInvestimentos,
                SaldoMensal = totalReceitas - totalDespesas, //- totalInvestimentos,
                MetasAtivas = metasAtivas.Take(5).Select(m => new MetaResumoDTO
                {
                    Id = m.Id,
                    Nome = m.Nome,
                    ValorObjetivo = m.ValorObjetivo,
                    ValorAtual = m.ValorAtual,
                    PercentualConcluido = m.PercentualConcluido(),
                    DataObjetivo = m.DataObjetivo
                }).ToList(),
                GraficoDespesas = await GetGraficoDespesasPorCategoriaAsync(mes, ano),
                GraficoReceitas = await GetGraficoReceitasPorCategoriaAsync(mes, ano)
            };
        }

        public async Task<List<GraficoDTO>> GetGraficoDespesasPorCategoriaAsync(int mes, int ano)
        {
            var despesas = await _despesaRepository.GetByMesAnoAsync(mes, ano, _currentUserService.UserId);
            var resumo = await GetDespesasPorCategoriaAsync(despesas);

            return resumo
                .Select(categoria => new GraficoDTO
                {
                    Label = categoria.NomeCategoria,
                    Valor = categoria.Total,
                    Cor = categoria.Cor
                })
                .ToList();
        }

        public async Task<List<GraficoDTO>> GetGraficoReceitasPorCategoriaAsync(int mes, int ano)
        {
            var receitas = await _receitaRepository.GetByMesAnoAsync(mes, ano, _currentUserService.UserId);
            var resumo = await GetReceitasPorCategoriaAsync(receitas);

            return resumo
                .Select(categoria => new GraficoDTO
                {
                    Label = categoria.NomeCategoria,
                    Valor = categoria.Total,
                    Cor = categoria.Cor
                })
                .ToList();
        }

        public async Task<BalancoMensalDTO> GetBalancoMensalAsync(int mes, int ano)
        {
            var usuarioId = _currentUserService.UserId;
            var receitas = await _receitaRepository.GetByMesAnoAsync(mes, ano, usuarioId);
            var despesas = await _despesaRepository.GetByMesAnoAsync(mes, ano, usuarioId);
            var investimentos = await _investimentoRepository.GetByPeriodoAsync(
                new DateTime(ano, mes, 1),
                new DateTime(ano, mes, DateTime.DaysInMonth(ano, mes)),
                usuarioId);

            var totalReceitas = receitas.Sum(r => r.Valor);
            var totalDespesas = despesas.Sum(d => d.Valor);
            //var totalInvestimentos = investimentos.Sum(i => i.ValorInvestido);

            return new BalancoMensalDTO
            {
                Mes = mes,
                Ano = ano,
                TotalReceitas = totalReceitas,
                TotalDespesas = totalDespesas,
                //TotalInvestimentos = totalInvestimentos,
                SaldoFinal = totalReceitas - totalDespesas, //- totalInvestimentos,
                DespesasPorCategoria = await GetDespesasPorCategoriaAsync(despesas),
                ReceitasPorCategoria = await GetReceitasPorCategoriaAsync(receitas.ToList())
            };
        }

        private async Task<List<CategoriaResumoDTO>> GetDespesasPorCategoriaAsync(List<Domain.Entities.Despesa> despesas)
        {
            if (despesas.Count == 0)
                return new List<CategoriaResumoDTO>();

            var categorias = await _categoriaRepository.GetAllAsync(_currentUserService.UserId);
            var categoriasPorId = categorias.ToDictionary(categoria => categoria.Id);

            return despesas
                .GroupBy(despesa => despesa.IdCategoria)
                .Select(grupo => new CategoriaResumoDTO
                {
                    CategoriaId = grupo.Key,
                    NomeCategoria = categoriasPorId.TryGetValue(grupo.Key, out var categoria)
                        ? categoria.Descricao
                        : "Categoria removida",
                    Total = grupo.Sum(despesa => despesa.Valor),
                    // Categoria ainda não possui cor persistida; manter o tom neutro usado pelo contrato antigo.
                    Cor = "#6c757d"
                })
                .OrderByDescending(c => c.Total)
                .ThenBy(c => c.NomeCategoria, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private async Task<List<CategoriaResumoDTO>> GetReceitasPorCategoriaAsync(List<Domain.Entities.Receita> receitas)
        {
            if (receitas.Count == 0)
                return new List<CategoriaResumoDTO>();

            // Receita não tem CategoriaId no modelo atual. Informar um total explicitamente
            // sem categoria evita inventar uma classificação ou retornar DTOs vazios por lançamento.
            return new List<CategoriaResumoDTO>
            {
                new()
                {
                    CategoriaId = Guid.Empty,
                    NomeCategoria = "Sem categoria",
                    Total = receitas.Sum(receita => receita.Valor),
                    Cor = "#28a745"
                }
            };
        }
    }
}
