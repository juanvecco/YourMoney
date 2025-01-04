using YourMoney.Domain.Models;

namespace YourMoney.Application.Interfaces
{
    public interface IDespesaService
    {
        Task AdicionarDespesaAsync(Despesa despesa);
    }
}
