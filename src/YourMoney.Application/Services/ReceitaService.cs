using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;
using YourMoney.Infrastructure.Repositories;

namespace YourMoney.Application.Services
{
    public class ReceitaService : IReceitaService
    {
        private readonly IReceitaRepository _receitaRepository;

        public ReceitaService(IReceitaRepository receitaRepository)
        {
            _receitaRepository = receitaRepository;
        }

        public async Task AdicionarReceitaAsync(Receita receita)
        {
            if (receita.Valor <= 0)
            {
                throw new ArgumentException("O valor da receita deve ser maior que zero.");
            }
            await _receitaRepository.AdicionarAsync(receita);
        }

        public async Task<Receita> GetReceitaByIdAsync(Guid id)
        {
            var receita = await _receitaRepository.GetByIdAsync(id);
            if (receita == null)
            {
                throw new InvalidOperationException("Receita não encontrada.");
            }
            return receita;
        }

        public async Task RemoverReceitaAsync(Guid id)
        {
            var receita = await _receitaRepository.GetByIdAsync(id);
            if (receita == null)
            {
                throw new InvalidOperationException("Receita não encontrada.");
            }
            await _receitaRepository.RemoverAsync(id);
        }

        public async Task AtualizarAsync(Receita receita)
        {
            var existingReceita = await _receitaRepository.GetByIdAsync(receita.Id);
            if (existingReceita == null)
            {
                throw new InvalidOperationException("Receita não encontrada.");
            }
            await _receitaRepository.AtualizarAsync(receita);
        }
        public async Task<List<Receita>> ListarAsync()
        {
            return await _receitaRepository.ListarAsync();
        }

        public async Task<List<Receita>> ObterPorMesAnoAsync(int mes, int ano)
        {
            var receitas = await _receitaRepository.ObterPorMesAnoAsync(mes, ano);
            return receitas.Where(r => r.Data.Month == mes && r.Data.Year == ano)
                           .ToList();
        }
    }
}
