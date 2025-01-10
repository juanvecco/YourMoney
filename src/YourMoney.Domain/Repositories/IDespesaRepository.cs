using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourMoney.Domain.Entities;

namespace YourMoney.Domain.Repositories
{
    public interface IDespesaRepository
    {
        //Task<Despesa> GetByIdAsync(Guid id);
        Task AddAsync(Despesa despesa);
        //Task UpdateAsync(Despesa despesa);
        //Task DeleteAsync(Guid id);
        Task<List<Despesa>> GetAllAsync();
    }
}
