using YourMoney.Domain.Entities;
using System.Threading.Tasks;

namespace YourMoney.Application.Interfaces
{
    public interface IDespesaService
    {
        Task AdicionarDespesaAsync(Despesa despesa);
    }
}
