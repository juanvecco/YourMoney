using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YourMoney.Domain.Entities;

namespace YourMoney.Application.Interfaces
{
    public interface IDespesaService
    {
        Task AdicionarDespesaAsync(Despesa despesa);
        Task<List<Despesa>> GetAllDespesasAsync(); // Método já existente
        Task<Despesa> GetDespesaByIdAsync(Guid id); // Método já existente
        Task RemoverDespesaAsync(Guid id); // Método já existente
        Task AtualizarAsync(Despesa despesa); // Novo método
        Task<List<Despesa>> ListarAsync(); // Novo método (alias para GetAllDespesasAsync)
    }
}