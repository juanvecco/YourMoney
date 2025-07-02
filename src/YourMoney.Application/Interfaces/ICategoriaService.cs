using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;

namespace YourMoney.Application.Interfaces
{
    public interface ICategoriaService
    {
        Task<Categoria> GetByIdAsync(Guid id);
        Task<List<Categoria>> GetAllAsync();
        Task<List<Categoria>> GetByTipoAsync(TipoTransacao tipo);
        Task<List<Categoria>> GetAtivasAsync();
        Task AdicionarAsync(Categoria categoria);
        Task AtualizarAsync(Categoria categoria);
        Task RemoverAsync(Guid id);
        Task<bool> ExisteAsync(Guid id);
        Task<bool> ExisteNomeAsync(string nome, TipoTransacao tipo, Guid? ignorarId = null);
    }
}