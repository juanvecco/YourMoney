using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Repositories;
using YourMoney.Domain.Entities;

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
            if (despesa.Valor.Valor <= 0)
            {
                throw new ArgumentException("O valor da despesa deve ser maior que zero.");
            }
            await _despesaRepository.AdicionarAsync(despesa);
        }

        public async Task<Despesa> GetDespesaByIdAsync(Guid id)
        {
            var despesa = await _despesaRepository.GetByIdAsync(id);
            if (despesa == null)
            {
                throw new InvalidOperationException("Despesa não encontrada.");
            }
            return despesa;
        }

        public async Task RemoverDespesaAsync(Guid id)
        {
            var despesa = await _despesaRepository.GetByIdAsync(id);
            if (despesa == null)
            {
                throw new InvalidOperationException("Despesa não encontrada.");
            }
            await _despesaRepository.RemoverAsync(id);
        }

        public async Task AtualizarAsync(Despesa despesa)
        {
            // Verifica se a despesa existe antes de atualizá-la
            var existingDespesa = await _despesaRepository.GetByIdAsync(despesa.Id);
            if (existingDespesa == null)
            {
                throw new InvalidOperationException("Despesa não encontrada.");
            }
            await _despesaRepository.AtualizarAsync(despesa);
        }
        public async Task<List<Despesa>> ListarAsync()
        {
            return await _despesaRepository.ListarAsync();
        }
    }
}