using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;
using YourMoney.Infrastructure.Repositories;

namespace YourMoney.Application.Services
{
    public class ContaFinanceiraService : IContaFinanceiraService
    {
        private readonly IContaFinanceiraRepository _contaFinanceiraRepository;
        private readonly ICurrentUserService _currentUserService;

        public ContaFinanceiraService(IContaFinanceiraRepository contaFinanceiraRepository, ICurrentUserService currentUserService)
        {
            _contaFinanceiraRepository = contaFinanceiraRepository;
            _currentUserService = currentUserService;
        }

        public async Task AdicionarContaFinanceiraAsync(ContaFinanceira contaFinanceira)
        {
            contaFinanceira.DefinirUsuario(_currentUserService.UserId);
            await _contaFinanceiraRepository.AdicionarAsync(contaFinanceira);
        }

        public async Task RemoverContaFinanceiraAsync(Guid id)
        {
            var contaFinanceira = await _contaFinanceiraRepository.GetByIdAsync(id, _currentUserService.UserId);
            if (contaFinanceira == null)
            {
                throw new InvalidOperationException("Conta Financeira não encontrada.");
            }
            await _contaFinanceiraRepository.RemoverAsync(id, _currentUserService.UserId);
        }

        public async Task AtualizarAsync(ContaFinanceira contaFinanceira)
        {
            var existeContaFinanceira = await _contaFinanceiraRepository.GetByIdAsync(contaFinanceira.Id, _currentUserService.UserId);
            if (existeContaFinanceira == null)
            {
                throw new InvalidOperationException("Conta Financeira não encontrada.");
            }
            contaFinanceira.DefinirUsuario(_currentUserService.UserId);
            await _contaFinanceiraRepository.AtualizarAsync(contaFinanceira);
        }
        public async Task<List<ContaFinanceira>> ListarAsync()
        {
            return await _contaFinanceiraRepository.ListarAsync(_currentUserService.UserId);
        }
        public async Task<ContaFinanceira> GetByIdAsync(Guid id)
        {
            var contaFinanceira = await _contaFinanceiraRepository.GetByIdAsync(id, _currentUserService.UserId);
            if (contaFinanceira == null)
            {
                throw new InvalidOperationException("Conta Financeira não encontrada.");
            }
            return contaFinanceira;
        }
    }
}
