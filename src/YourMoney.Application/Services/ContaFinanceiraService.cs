using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;

namespace YourMoney.Application.Services
{
    public class ContaFinanceiraService : IContaFinanceiraService
    {
        private readonly IContaFinanceiraRepository _contaFinanceiraRepository;

        public ContaFinanceiraService(IContaFinanceiraRepository contaFinanceiraRepository)
        {
            _contaFinanceiraRepository = contaFinanceiraRepository;
        }

        public async Task AdicionarContaFinanceiraAsync(ContaFinanceira contaFinanceira)
        {
            await _contaFinanceiraRepository.AdicionarAsync(contaFinanceira);
        }

        public async Task RemoverContaFinanceiraAsync(Guid id)
        {
            var contaFinanceira = await _contaFinanceiraRepository.GetByIdAsync(id);
            if (contaFinanceira == null)
            {
                throw new InvalidOperationException("Conta Financeira não encontrada.");
            }
            await _contaFinanceiraRepository.RemoverAsync(id);
        }

        public async Task AtualizarAsync(ContaFinanceira contaFinanceira)
        {
            var existeContaFinanceira = await _contaFinanceiraRepository.GetByIdAsync(contaFinanceira.Id);
            if (existeContaFinanceira == null)
            {
                throw new InvalidOperationException("Conta Financeira não encontrada.");
            }
            await _contaFinanceiraRepository.AtualizarAsync(contaFinanceira);
        }
        public async Task<List<ContaFinanceira>> ListarAsync()
        {
            return await _contaFinanceiraRepository.ListarAsync();
        }
    }
}
