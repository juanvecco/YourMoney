using Microsoft.EntityFrameworkCore;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;
using YourMoney.Domain.Repositories;
using YourMoney.Infrastructure.Persistence;

namespace YourMoney.Infrastructure.Repositories
{
    public class MetaRepository : IMetaRepository
    {
        private readonly AppDbContext _context;

        public MetaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Meta> GetByIdAsync(Guid id)
        {
            return await _context.Metas
                .Include(m => m.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<Meta>> GetAllAsync()
        {
            return await _context.Metas
                .Include(m => m.Categoria)
                .OrderByDescending(m => m.DataInicio)
                .ToListAsync();
        }

        public async Task<List<Meta>> GetByStatusAsync(StatusMeta status)
        {
            return await _context.Metas
                .Include(m => m.Categoria)
                .Where(m => m.Status == status)
                .OrderByDescending(m => m.DataInicio)
                .ToListAsync();
        }

        public async Task<List<Meta>> GetAtivasAsync()
        {
            return await _context.Metas
                .Include(m => m.Categoria)
                .Where(m => m.Status == StatusMeta.Ativa)
                .OrderBy(m => m.DataObjetivo)
                .ToListAsync();
        }

        public async Task<List<Meta>> GetVencendoAsync(int diasProximoVencimento = 30)
        {
            var dataLimite = DateTime.Now.AddDays(diasProximoVencimento);

            return await _context.Metas
                .Include(m => m.Categoria)
                .Where(m => m.Status == StatusMeta.Ativa && m.DataObjetivo <= dataLimite)
                .OrderBy(m => m.DataObjetivo)
                .ToListAsync();
        }

        public async Task AdicionarAsync(Meta meta)
        {
            await _context.Metas.AddAsync(meta);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Meta meta)
        {
            _context.Metas.Update(meta);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(Guid id)
        {
            var meta = await GetByIdAsync(id);
            if (meta != null)
            {
                _context.Metas.Remove(meta);
                await _context.SaveChangesAsync();
            }
        }
    }
}