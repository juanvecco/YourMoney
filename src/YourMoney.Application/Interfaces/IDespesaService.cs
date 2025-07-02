using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YourMoney.Domain.Entities;

namespace YourMoney.Application.Interfaces
{
    public interface IDespesaService
    {
        Task AdicionarDespesaAsync(Despesa despesa);
        Task<Despesa> GetDespesaByIdAsync(Guid id); // Método já existente
        Task RemoverDespesaAsync(Guid id); // Método já existente
        Task AtualizarAsync(Despesa despesa); // Novo método
    }
}