using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourMoney.Domain.Entities;

namespace YourMoney.Application.Interfaces
{
    public interface IReceitaService
    {
        Task AdicionarReceitaAsync(Receita receita);
        //Task<Despesa> GetDespesaByIdAsync(Guid id); // Método já existente
        //Task RemoverDespesaAsync(Guid id); // Método já existente
        //Task AtualizarAsync(Despesa despesa); // Novo método
        //Task<List<Despesa>> ListarAsync();
    }
}
