using YourMoney.Domain.Entities;

namespace YourMoney.Domain.Interfaces.Repositories
{
    public interface IDespesaRepository
    {
        Task AdicionarAsync(Despesa despesa);
        // Outros métodos, como Buscar, Atualizar, Deletar
    }
}
