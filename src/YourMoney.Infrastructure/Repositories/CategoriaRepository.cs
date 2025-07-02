using Microsoft.EntityFrameworkCore;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;
using YourMoney.Domain.Repositories;
using YourMoney.Infrastructure.Persistence;

namespace YourMoney.Infrastructure.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly AppDbContext _context;

        public CategoriaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Categoria> GetByIdAsync(Guid id)
        {
            return await _context.Categorias.FindAsync(id);
        }

        public async Task<List<Categoria>> GetAllAsync()
        {
            return await _context.Categorias.OrderBy(c => c.Nome).ToListAsync();
        }

        public async Task<List<Categoria>> GetByTipoAsync(TipoTransacao tipo)
        {
            return await _context.Categorias
                .Where(c => c.TipoTransacao == tipo)
                .OrderBy(c => c.Nome)
                .ToListAsync();
        }

        public async Task<List<Categoria>> GetAtivasAsync()
        {
            return await _context.Categorias
                .Where(c => c.Ativa)
                .OrderBy(c => c.Nome)
                .ToListAsync();
        }

        public async Task AdicionarAsync(Categoria categoria)
        {
            await _context.Categorias.AddAsync(categoria);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Categoria categoria)
        {
            _context.Categorias.Update(categoria);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(Guid id)
        {
            var categoria = await GetByIdAsync(id);
            if (categoria != null)
            {
                _context.Categorias.Remove(categoria);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExisteAsync(Guid id)
        {
            return await _context.Categorias.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> ExisteNomeAsync(string nome, TipoTransacao tipo, Guid? ignorarId = null)
        {
            var query = _context.Categorias.Where(c => c.Nome == nome && c.TipoTransacao == tipo);

            if (ignorarId.HasValue)
                query = query.Where(c => c.Id != ignorarId.Value);


            return await query.AnyAsync();
        }
    }
}