using YourMoney.Application.Interfaces;
using YourMoney.Domain.Repositories;
using YourMoney.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            // Validações adicionais podem ser feitas aqui
            if (despesa.Valor <= 0)
            {
                throw new ArgumentException("O valor da despesa deve ser maior que zero.");
            }
            await _despesaRepository.AdicionarAsync(despesa);
        }

        public async Task<List<Despesa>> GetAllDespesasAsync()
        {
            // Chama o repositório para obter todas as despesas
            return await _despesaRepository.GetAllAsync();
        }

        public async Task<Despesa> GetDespesaByIdAsync(Guid id)
        {
            // Chama o repositório para buscar uma despesa pelo ID
            var despesa = await _despesaRepository.GetByIdAsync(id);
            if (despesa == null)
            {
                throw new InvalidOperationException("Despesa não encontrada.");
            }
            return despesa;
        }

        public async Task RemoverDespesaAsync(Guid id)
        {
            // Verifica se a despesa existe antes de tentar removê-la
            var despesa = await _despesaRepository.GetByIdAsync(id);
            if (despesa == null)
            {
                throw new InvalidOperationException("Despesa não encontrada.");
            }
            // Chama o método de remoção no repositório
            await _despesaRepository.RemoverAsync(id);
        }
    }
}