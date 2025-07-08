using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;

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
    }
}
