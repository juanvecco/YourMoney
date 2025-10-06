using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;

namespace YourMoney.Application.Services
{
    public class InvestimentoService : IInvestimentoService
    {
        private readonly IInvestimentoRepository _investimentoRepository;
        public InvestimentoService(IInvestimentoRepository investimentoRepository)
        {
            _investimentoRepository = investimentoRepository;
        }

        public async Task AdicionarInvestimentoAsync(Investimento investimento)
        {
            if (investimento.ValorAtual <= 0)
            {
                throw new ArgumentException("O valor do investimento deve ser maior que zero.");
            }
            await _investimentoRepository.AdicionarAsync(investimento);
        }

        public async Task<Investimento> GetInvestimentoByIdAsync(Guid id)
        {
            var investimento = await _investimentoRepository.GetByIdAsync(id);
            if (investimento == null)
            {
                throw new InvalidOperationException("Investimento não encontrada.");
            }
            return investimento;
        }

        public async Task RemoverInvestimentoAsync(Guid id)
        {
            var investimento = await _investimentoRepository.GetByIdAsync(id);
            if (investimento == null)
            {
                throw new InvalidOperationException("Investimento não encontrada.");
            }
            await _investimentoRepository.RemoverAsync(id);
        }

        public async Task AtualizarAsync(Investimento investimento)
        {
            var existingInvestimento = await _investimentoRepository.GetByIdAsync(investimento.Id);
            if (existingInvestimento == null)
            {
                throw new InvalidOperationException("Investimento não encontrada.");
            }
            await _investimentoRepository.AtualizarAsync(investimento);
        }
        public async Task<List<Investimento>> ListarAsync()
        {
            return await _investimentoRepository.ListarAsync();
        }

        public async Task<List<Investimento>> ObterPorMesAnoAsync(int mes, int ano)
        {
            var investimentos = await _investimentoRepository.ObterPorMesAnoAsync(mes, ano);
            return investimentos.Where(r => r.DataInvestimento.Month == mes && r.DataInvestimento.Year == ano)
                           .ToList();
        }
    }
}
