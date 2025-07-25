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

        public DashboardService(
            IReceitaRepository receitaRepository,
            IDespesaRepository despesaRepository,
            IInvestimentoRepository investimentoRepository,
            IMetaRepository metaRepository,
            ICategoriaRepository categoriaRepository)
        {
            _receitaRepository = receitaRepository;
            _despesaRepository = despesaRepository;
            _investimentoRepository = investimentoRepository;
            _metaRepository = metaRepository;
            _categoriaRepository = categoriaRepository;
        }

        public async Task<DashboardDTO> GetDashboardDataAsync(int mes, int ano)
        {
            var receitas = await _receitaRepository.GetByMesAnoAsync(mes, ano);
            var despesas = await _despesaRepository.GetByMesAnoAsync(mes, ano);
            var investimentos = await _investimentoRepository.GetAllAsync();
            var metasAtivas = await _metaRepository.GetAtivasAsync();

            var totalReceitas = receitas.Sum(r => r.Valor);
            var totalDespesas = despesas.Sum(d => d.Valor);
            var totalInvestimentos = investimentos.Sum(i => i.ValorInvestido);

            return new DashboardDTO
            {
                TotalReceitas = totalReceitas,
                TotalDespesas = totalDespesas,
                TotalInvestimentos = totalInvestimentos,
                SaldoMensal = totalReceitas - totalDespesas - totalInvestimentos,
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
            var despesas = await _despesaRepository.GetByMesAnoAsync(mes, ano);

            return despesas
                //.GroupBy(d => d.CategoriaId)
                .Select(g => new GraficoDTO
                {
                    //Label = categorias.FirstOrDefault(c => c.Id == g.Key)?.Nome ?? "Sem Categoria",
                    //Valor = g.Sum(d => d.Valor),
                    //Cor = categorias.FirstOrDefault(c => c.Id == g.Key)?.Cor ?? "#6c757d"
                })
                .OrderByDescending(g => g.Valor)
                .ToList();
        }

        public async Task<List<GraficoDTO>> GetGraficoReceitasPorCategoriaAsync(int mes, int ano)
        {
            var receitas = await _receitaRepository.GetByMesAnoAsync(mes, ano);

            return receitas
                //.Where(r => r.Recebida)
                //.GroupBy(r => r.CategoriaId)
                .Select(g => new GraficoDTO
                {
                    //Label = categorias.FirstOrDefault(c => c.Id == g.Key)?.Nome ?? "Sem Categoria",
                    //Valor = g.Sum(r => r.Valor.Valor),
                    //Cor = categorias.FirstOrDefault(c => c.Id == g.Key)?.Cor ?? "#28a745"
                })
                .OrderByDescending(g => g.Valor)
                .ToList();
        }

        public async Task<BalancoMensalDTO> GetBalancoMensalAsync(int mes, int ano)
        {
            var receitas = await _receitaRepository.GetByMesAnoAsync(mes, ano);
            var despesas = await _despesaRepository.GetByMesAnoAsync(mes, ano);
            var investimentos = await _investimentoRepository.GetByPeriodoAsync(
                new DateTime(ano, mes, 1),
                new DateTime(ano, mes, DateTime.DaysInMonth(ano, mes)));

            var totalReceitas = receitas.Sum(r => r.Valor);
            var totalDespesas = despesas.Sum(d => d.Valor);
            var totalInvestimentos = investimentos.Sum(i => i.ValorInvestido);

            return new BalancoMensalDTO
            {
                Mes = mes,
                Ano = ano,
                TotalReceitas = totalReceitas,
                TotalDespesas = totalDespesas,
                TotalInvestimentos = totalInvestimentos,
                SaldoFinal = totalReceitas - totalDespesas - totalInvestimentos,
                DespesasPorCategoria = await GetDespesasPorCategoriaAsync(despesas),
                ReceitasPorCategoria = await GetReceitasPorCategoriaAsync(receitas.ToList())
            };
        }

        private async Task<List<CategoriaResumoDTO>> GetDespesasPorCategoriaAsync(List<Domain.Entities.Despesa> despesas)
        {

            return despesas
                //.GroupBy(d => d.CategoriaId)
                .Select(g => new CategoriaResumoDTO
                {
                    //CategoriaId = g.Key,
                    //NomeCategoria = categorias.FirstOrDefault(c => c.Id == g.Key)?.Nome ?? "Sem Categoria",
                    //Total = g.Sum(d => d.Valor),
                    //Cor = categorias.FirstOrDefault(c => c.Id == g.Key)?.Cor ?? "#6c757d"
                })
                .OrderByDescending(c => c.Total)
                .ToList();
        }

        private async Task<List<CategoriaResumoDTO>> GetReceitasPorCategoriaAsync(List<Domain.Entities.Receita> receitas)
        {

            return receitas
                //.GroupBy(r => r.CategoriaId)
                .Select(g => new CategoriaResumoDTO
                {
                    //CategoriaId = g.Key,
                    //NomeCategoria = categorias.FirstOrDefault(c => c.Id == g.Key)?.Nome ?? "Sem Categoria",
                    //Total = g.Sum(r => r.Valor.Valor),
                   // Cor = categorias.FirstOrDefault(c => c.Id == g.Key)?.Cor ?? "#28a745"
                })
                .OrderByDescending(c => c.Total)
                .ToList();
        }
    }
}