using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;

namespace YourMoney.Application.Services
{
    public class InvestimentoService : IInvestimentoService
    {
        private readonly IInvestimentoRepository _investimentoRepository;
        private readonly ICurrentUserService _currentUserService;
        public InvestimentoService(IInvestimentoRepository investimentoRepository, ICurrentUserService currentUserService)
        {
            _investimentoRepository = investimentoRepository;
            _currentUserService = currentUserService;
        }

        public async Task AdicionarInvestimentoAsync(Investimento investimento)
        {
            if (investimento.ValorAtual <= 0)
            {
                throw new ArgumentException("O valor do investimento deve ser maior que zero.");
            }
            investimento.DefinirUsuario(_currentUserService.UserId);
            await _investimentoRepository.AdicionarAsync(investimento);
        }

        public async Task<Investimento> GetInvestimentoByIdAsync(Guid id)
        {
            var investimento = await _investimentoRepository.GetByIdAsync(id, _currentUserService.UserId);
            if (investimento == null)
            {
                throw new InvalidOperationException("Investimento não encontrada.");
            }
            return investimento;
        }

        public async Task RemoverInvestimentoAsync(Guid id)
        {
            var investimento = await _investimentoRepository.GetByIdAsync(id, _currentUserService.UserId);
            if (investimento == null)
            {
                throw new InvalidOperationException("Investimento não encontrada.");
            }
            await _investimentoRepository.RemoverAsync(id, _currentUserService.UserId);
        }

        public async Task AtualizarAsync(Investimento investimento)
        {
            var existingInvestimento = await _investimentoRepository.GetByIdAsync(investimento.Id, _currentUserService.UserId);
            if (existingInvestimento == null)
            {
                throw new InvalidOperationException("Investimento não encontrada.");
            }
            investimento.DefinirUsuario(_currentUserService.UserId);
            await _investimentoRepository.AtualizarAsync(investimento);
        }
        public async Task<List<Investimento>> ListarAsync()
        {
            return await _investimentoRepository.ListarAsync(_currentUserService.UserId);
        }

        public async Task<List<Investimento>> ObterPorMesAnoAsync(int mes, int ano)
        {
            var investimentos = await _investimentoRepository.ObterPorMesAnoAsync(mes, ano, _currentUserService.UserId);
            return investimentos.Where(r => r.DataInvestimento.Month == mes && r.DataInvestimento.Year == ano)
                           .ToList();
        }
    }
}
