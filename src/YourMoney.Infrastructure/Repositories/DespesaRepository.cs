using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using YourMoney.Infrastructure.Persistence;

namespace YourMoney.Infrastructure.Repositories
{
    public class DespesaRepository : IDespesaRepository
    {
        private readonly AppDbContext _context;

        public DespesaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AdicionarAsync(Despesa despesa)
        {
            await _context.TbDespesa.AddAsync(despesa);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Despesa>> GetAllAsync()
        {
            // Exemplo usando Entity Framework
            return await _context.TbDespesa.ToListAsync();
        }

        public async Task RemoverAsync(Guid id)
        {
            var despesa = await _context.TbDespesa.FindAsync(id);
            if (despesa == null)
            {
                throw new InvalidOperationException("Despesa não encontrada.");
            }
            // Remove a despesa do contexto
            _context.TbDespesa.Remove(despesa);
            // Salva as alterações no banco de dados
            await _context.SaveChangesAsync();
        }

        public async Task<Despesa> GetByIdAsync(Guid id)
        {
            var despesa = await _context.TbDespesa.FindAsync(id);
            if (despesa == null)
            {
                throw new InvalidOperationException("Despesa não encontrada.");
            }
            return despesa;
        }

        public async Task UpdateAsync(Despesa despesa)
        {
            // Verifica se a despesa existe no banco de dados
            var existingDespesa = await _context.TbDespesa.FindAsync(despesa.Id);
            if (existingDespesa == null)
            {
                throw new InvalidOperationException("Despesa não encontrada.");
            }

            // Atualiza as propriedades da entidade existente
            _context.Entry(existingDespesa).CurrentValues.SetValues(despesa);

            // Salva as alterações no banco de dados
            await _context.SaveChangesAsync();
        }
    }
}