using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;
using YourMoney.Domain.Repositories;
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

        public async Task<Despesa> GetByIdAsync(Guid id)
        {
            var despesa = await _context.Despesas
                //.Include(r => r.Categoria)
                .FirstOrDefaultAsync(r => r.Id == id);
            return despesa == null ? throw new InvalidOperationException("Despesa não encontrada.") : despesa;
        }

        public async Task<Despesa> GetByIdAsync(Guid id, string usuarioId)
        {
            var despesa = await _context.Despesas
                .FirstOrDefaultAsync(r => r.Id == id && r.UsuarioId == usuarioId);
            return despesa == null ? throw new InvalidOperationException("Despesa não encontrada.") : despesa;
        }

        public async Task<List<Despesa>> GetAllAsync()
        {
            return await _context.Despesas
                //.Include(r => r.Categoria)
                .OrderByDescending(r => r.Data)
                .ToListAsync();
        }

        public async Task<List<Despesa>> GetAllAsync(string usuarioId)
        {
            return await _context.Despesas
                .Where(r => r.UsuarioId == usuarioId)
                .OrderByDescending(r => r.Data)
                .ToListAsync();
        }

        public async Task<List<Despesa>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim)
        {
            return await _context.Despesas
                //.Include(r => r.Categoria)
                .Where(r => r.Data >= dataInicio && r.Data <= dataFim)
                .OrderByDescending(r => r.Data)
                .ToListAsync();
        }

        public async Task<List<Despesa>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim, string usuarioId)
        {
            return await _context.Despesas
                .Where(r => r.UsuarioId == usuarioId && r.Data >= dataInicio && r.Data <= dataFim)
                .OrderByDescending(r => r.Data)
                .ToListAsync();
        }

        public async Task<List<Despesa>> GetByReferenciaPeriodoAsync(
            DateTime inicio,
            DateTime fimExclusivo,
            string usuarioId)
        {
            return await _context.Despesas
                .AsNoTracking()
                .Include(d => d.ContaFinanceira)
                .Include(d => d.Categoria)
                .Where(d => d.UsuarioId == usuarioId
                    && d.MesReferencia >= inicio
                    && d.MesReferencia < fimExclusivo)
                .ToListAsync();
        }

        public async Task<List<Despesa>> GetByMesAnoAsync(int mes, int ano)
        {
            return await _context.Despesas
                //.Include(r => r.Categoria)
                .Where(r => r.MesReferencia.Month == mes && r.MesReferencia.Year == ano)
                .OrderByDescending(r => r.Data)
                .ToListAsync();
        }

        public async Task<List<Despesa>> GetByMesAnoAsync(int mes, int ano, string usuarioId)
        {
            return await _context.Despesas
                .Where(r => r.UsuarioId == usuarioId && r.MesReferencia.Month == mes && r.MesReferencia.Year == ano)
                .OrderByDescending(r => r.Data)
                .ToListAsync();
        }

        public async Task<List<Despesa>> ObterPorMesAnoAsync(int mes, int ano, Guid? idContaFinanceira = null)
        {
            return await _context.Despesas
                .Where(d => d.MesReferencia.Month == mes && d.MesReferencia.Year == ano
                            && (idContaFinanceira == null || d.IdContaFinanceira == idContaFinanceira))
                .ToListAsync();
        }

        public async Task<List<Despesa>> ObterPorMesAnoAsync(int mes, int ano, string usuarioId, Guid? idContaFinanceira = null)
        {
            return await _context.Despesas
                .Where(d => d.UsuarioId == usuarioId
                            && d.MesReferencia.Month == mes && d.MesReferencia.Year == ano
                            && (idContaFinanceira == null || d.IdContaFinanceira == idContaFinanceira))
                .ToListAsync();
        }

        public async Task<List<Despesa>> ConsultarAsync(
            int mes,
            int ano,
            string usuarioId,
            Guid? idContaFinanceira,
            IReadOnlyCollection<Guid> idsCategoria)
        {
            var query = _context.Despesas
                .Where(d => d.UsuarioId == usuarioId
                            && d.MesReferencia.Month == mes
                            && d.MesReferencia.Year == ano
                            && (idContaFinanceira == null || d.IdContaFinanceira == idContaFinanceira));

            if (idsCategoria != null)
            {
                query = query.Where(d => idsCategoria.Contains(d.IdCategoria));
            }

            return await query
                .OrderByDescending(d => d.Data)
                .ThenByDescending(d => d.Id)
                .ToListAsync();
        }

        public async Task<ConsultaDespesasPaginada> ConsultarPaginadoAsync(
            int mes, int ano, string usuarioId, Guid? idContaFinanceira,
            IReadOnlyCollection<Guid> idsCategoria, int pagina, int tamanhoPagina)
        {
            var inicio = new DateTime(ano, mes, 1);
            var fimExclusivo = inicio.AddMonths(1);
            var query = _context.Despesas.AsNoTracking()
                .Where(d => d.UsuarioId == usuarioId
                    && d.MesReferencia >= inicio && d.MesReferencia < fimExclusivo
                    && (idContaFinanceira == null || d.IdContaFinanceira == idContaFinanceira));

            if (idsCategoria != null)
                query = query.Where(d => idsCategoria.Contains(d.IdCategoria));

            var gastosPorConta = await query
                .GroupBy(d => d.IdContaFinanceira)
                .Select(grupo => new
                {
                    IdContaFinanceira = grupo.Key,
                    Quantidade = grupo.Count(),
                    ValorBruto = grupo.Sum(d => d.Valor)
                })
                .ToListAsync();

            var reembolsosPorConta = await (
                from despesa in query
                join receita in _context.Receitas.AsNoTracking()
                    .Where(r => r.UsuarioId == usuarioId && r.Natureza == NaturezaReceita.Reembolso)
                    on (Guid?)despesa.Id equals receita.DespesaVinculadaId
                group receita by despesa.IdContaFinanceira into grupo
                select new { IdContaFinanceira = grupo.Key, Valor = grupo.Sum(r => r.Valor) }
            ).ToDictionaryAsync(total => total.IdContaFinanceira, total => total.Valor);

            var totalResultados = gastosPorConta.Sum(total => total.Quantidade);
            var totalPaginas = totalResultados == 0 ? 0 : (int)Math.Ceiling(totalResultados / (decimal)tamanhoPagina);
            var paginaAtual = totalPaginas == 0 ? 1 : Math.Min(Math.Max(pagina, 1), totalPaginas);
            var itens = totalResultados == 0
                ? new List<Despesa>()
                : await query.OrderByDescending(d => d.Data).ThenByDescending(d => d.Id)
                    .Skip((paginaAtual - 1) * tamanhoPagina).Take(tamanhoPagina).ToListAsync();

            return new ConsultaDespesasPaginada(itens, paginaAtual, totalResultados,
                gastosPorConta.Select(total => new TotalDespesasPorConta(total.IdContaFinanceira,
                    total.Quantidade, Math.Max(0m, total.ValorBruto - reembolsosPorConta.GetValueOrDefault(total.IdContaFinanceira)))).ToList());
        }

        public async Task<bool> ExisteAsync(Guid id, string usuarioId)
        {
            return await _context.Despesas
                .AnyAsync(d => d.Id == id && d.UsuarioId == usuarioId);
        }

        public async Task<List<Despesa>> GetByCategoriaAsync(Guid categoriaId)
        {
            return await _context.Despesas
                //.Include(r => r.Categoria)
                //.Where(r => r.CategoriaId == categoriaId)
                .OrderByDescending(r => r.Data)
                .ToListAsync();
        }

        public async Task<List<Despesa>> GetByCategoriaAsync(Guid categoriaId, string usuarioId)
        {
            return await _context.Despesas
                .Where(r => r.UsuarioId == usuarioId && r.IdCategoria == categoriaId)
                .OrderByDescending(r => r.Data)
                .ToListAsync();
        }

        public async Task AdicionarAsync(Despesa despesa)
        {
            await _context.Despesas.AddAsync(despesa);
            await _context.SaveChangesAsync();
        }

        public async Task AdicionarEmLoteAsync(IReadOnlyCollection<Despesa> despesas)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Despesas.AddRangeAsync(despesas);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task AtualizarAsync(Despesa despesa)
        {
            _context.Despesas.Update(despesa);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(Guid id)
        {
            var despesa = await GetByIdAsync(id);
            if (despesa != null)
            {
                _context.Despesas.Remove(despesa);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoverAsync(Guid id, string usuarioId)
        {
            var despesa = await GetByIdAsync(id, usuarioId);
            _context.Despesas.Remove(despesa);
            await _context.SaveChangesAsync();
        }
        public async Task<List<Despesa>> ListarAsync()
        {
            return await _context.Despesas
                //.Include(d => d.Categoria)
                .ToListAsync();
        }

        public async Task<List<Despesa>> ListarAsync(string usuarioId)
        {
            return await _context.Despesas
                .Where(d => d.UsuarioId == usuarioId)
                .ToListAsync();
        }
    }
}
