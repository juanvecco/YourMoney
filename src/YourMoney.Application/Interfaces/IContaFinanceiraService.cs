using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourMoney.Domain.Entities;

namespace YourMoney.Application.Interfaces
{
    public interface IContaFinanceiraService
    {
        Task AdicionarContaFinanceiraAsync(ContaFinanceira contaFinanceira);
        Task RemoverContaFinanceiraAsync(Guid id);
        Task AtualizarAsync(ContaFinanceira contaFinanceira);
        Task<List<ContaFinanceira>> ListarAsync();
        Task<ContaFinanceira> GetByIdAsync(Guid id);
    }
}
