using YourMoney.Application.Interfaces;
using YourMoney.Domain.Interfaces.Repositories;
using YourMoney.Domain.Models;

namespace YourMoney.Application.Services
{
    public class DespesaService : IDespesaService
    {
        private readonly IDespesaRepository _despesaRepository;

        public DespesaService(IDespesaRepository despesaRepository)
        {
            _despesaRepository = despesaRepository;
        }

        public async Task AdicionarDespesaAsync(Despesa despesa)
        {
            // Adicione validações aqui, se necessário
            if (despesa.Valor <= 0)
            {
                throw new ArgumentException("O valor da despesa deve ser maior que zero.");
            }

            despesa.Data = despesa.Data == default ? DateTime.UtcNow : despesa.Data;
            await _despesaRepository.AdicionarAsync(despesa);
        }
    }
}
