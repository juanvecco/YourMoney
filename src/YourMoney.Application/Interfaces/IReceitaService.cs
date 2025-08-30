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
        Task<Receita> GetReceitaByIdAsync(Guid id);
        Task RemoverReceitaAsync(Guid id);
        Task AtualizarAsync(Receita receita);
        Task<List<Receita>> ListarAsync();
        Task<List<Receita>> ObterPorMesAnoAsync(int mes, int ano);
    }
}
