using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourMoney.Application.DTOs;

namespace YourMoney.Application.Interfaces
{
    public interface IContaFinanceiraService
    {
        Task<ContaFinanceiraResponse> CriarAsync(ContaFinanceiraRequest request);
        Task RemoverContaFinanceiraAsync(Guid id);
        Task<ContaFinanceiraResponse> AtualizarAsync(Guid id, ContaFinanceiraRequest request);
        Task<List<ContaFinanceiraResponse>> ListarAsync();
        Task<ContaFinanceiraResponse> GetByIdAsync(Guid id);
    }
}
