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

        public async Task<Categoria> GetByIdAsync(Guid id, string usuarioId)
        {
            return await _context.Categorias
                .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == usuarioId);
        }

        public async Task<List<Categoria>> GetAllAsync()
        {
            return await _context.Categorias.OrderBy(c => c.Descricao).ToListAsync();
        }

        public async Task<List<Categoria>> GetAllAsync(string usuarioId)
        {
            return await _context.Categorias
                .Where(c => c.UsuarioId == usuarioId)
                .OrderBy(c => c.Descricao)
                .ToListAsync();
        }

        public async Task<bool> ExisteAsync(Guid id)
        {
            return await _context.Categorias.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> ExisteAsync(Guid id, string usuarioId)
        {
            return await _context.Categorias.AnyAsync(c => c.Id == id && c.UsuarioId == usuarioId);
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

        public async Task RemoverAsync(Guid id, string usuarioId)
        {
            var categoria = await GetByIdAsync(id, usuarioId);
            if (categoria != null)
            {
                _context.Categorias.Remove(categoria);
                await _context.SaveChangesAsync();
            }
        }
    }
}
