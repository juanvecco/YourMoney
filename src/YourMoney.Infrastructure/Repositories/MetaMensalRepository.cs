using Microsoft.EntityFrameworkCore;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;
using YourMoney.Infrastructure.Persistence;

#nullable enable

namespace YourMoney.Infrastructure.Repositories
{
    public class MetaMensalRepository : IMetaMensalRepository
    {
        private readonly AppDbContext _context;

        public MetaMensalRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<MetaMensal?> GetByIdAsync(Guid id, string usuarioId)
        {
            return await _context.MetasMensais
                .FirstOrDefaultAsync(meta => meta.Id == id && meta.UsuarioId == usuarioId);
        }

        public async Task<List<MetaMensal>> ObterPorMesAnoAsync(int mes, int ano, string usuarioId)
        {
            return await _context.MetasMensais
                .Where(meta => meta.UsuarioId == usuarioId
                    && meta.MesReferencia.Month == mes
                    && meta.MesReferencia.Year == ano)
                .OrderBy(meta => meta.Nome)
                .ToListAsync();
        }

        public async Task AdicionarAsync(MetaMensal meta)
        {
            await _context.MetasMensais.AddAsync(meta);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(MetaMensal meta)
        {
            _context.MetasMensais.Update(meta);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(Guid id, string usuarioId)
        {
            var meta = await GetByIdAsync(id, usuarioId);
            if (meta == null)
                return;

            _context.MetasMensais.Remove(meta);
            await _context.SaveChangesAsync();
        }
    }
}
