#nullable enable

using Microsoft.EntityFrameworkCore;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;
using YourMoney.Infrastructure.Persistence;

namespace YourMoney.Infrastructure.Repositories
{
    public class DespesaRecorrenteRepository : IDespesaRecorrenteRepository
    {
        private readonly AppDbContext _context;

        public DespesaRecorrenteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AdicionarAsync(DespesaRecorrente recorrencia)
        {
            await _context.DespesasRecorrentes.AddAsync(recorrencia);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(DespesaRecorrente recorrencia)
        {
            _context.DespesasRecorrentes.Update(recorrencia);
            await _context.SaveChangesAsync();
        }

        public async Task<DespesaRecorrente?> ObterPorIdAsync(Guid id, string usuarioId)
        {
            return await RecorrenciasComRelacionamentos()
                .FirstOrDefaultAsync(r => r.Id == id && r.UsuarioId == usuarioId);
        }

        public async Task<List<DespesaRecorrente>> ListarAsync(string usuarioId, bool? ativas = null)
        {
            var query = RecorrenciasComRelacionamentos()
                .Where(r => r.UsuarioId == usuarioId);

            if (ativas.HasValue)
                query = query.Where(r => r.Ativa == ativas.Value);

            return await query
                .OrderBy(r => r.Descricao)
                .ToListAsync();
        }

        public async Task<List<DespesaRecorrente>> ListarElegiveisParaMesAsync(string usuarioId, DateTime mesReferencia)
        {
            var mes = DespesaRecorrente.NormalizarMesReferencia(mesReferencia);
            var fimDoMes = new DateTime(mes.Year, mes.Month, DateTime.DaysInMonth(mes.Year, mes.Month));

            return await RecorrenciasComRelacionamentos()
                .Where(r => r.UsuarioId == usuarioId
                            && r.Ativa
                            && r.DataInicio <= fimDoMes
                            && (r.DataTermino == null || r.DataTermino >= mes))
                .OrderBy(r => r.Descricao)
                .ToListAsync();
        }

        public async Task<DespesaRecorrenteOcorrencia?> ObterOcorrenciaAsync(
            Guid despesaRecorrenteId,
            DateTime mesReferencia,
            string usuarioId)
        {
            var mes = DespesaRecorrente.NormalizarMesReferencia(mesReferencia);
            return await OcorrenciasComRelacionamentos()
                .FirstOrDefaultAsync(o => o.UsuarioId == usuarioId
                                          && o.DespesaRecorrenteId == despesaRecorrenteId
                                          && o.MesReferencia == mes);
        }

        public async Task<DespesaRecorrenteOcorrencia?> ObterOcorrenciaPorIdAsync(Guid ocorrenciaId, string usuarioId)
        {
            return await OcorrenciasComRelacionamentos()
                .FirstOrDefaultAsync(o => o.Id == ocorrenciaId && o.UsuarioId == usuarioId);
        }

        public async Task<List<DespesaRecorrenteOcorrencia>> ListarOcorrenciasPorMesAsync(string usuarioId, DateTime mesReferencia)
        {
            var mes = DespesaRecorrente.NormalizarMesReferencia(mesReferencia);
            return await OcorrenciasComRelacionamentos()
                .Where(o => o.UsuarioId == usuarioId && o.MesReferencia == mes)
                .OrderBy(o => o.DespesaRecorrente.Descricao)
                .ToListAsync();
        }

        public async Task AdicionarOcorrenciaAsync(DespesaRecorrenteOcorrencia ocorrencia)
        {
            await _context.DespesasRecorrentesOcorrencias.AddAsync(ocorrencia);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarOcorrenciaAsync(DespesaRecorrenteOcorrencia ocorrencia)
        {
            _context.DespesasRecorrentesOcorrencias.Update(ocorrencia);
            await _context.SaveChangesAsync();
        }

        private IQueryable<DespesaRecorrente> RecorrenciasComRelacionamentos()
        {
            return _context.DespesasRecorrentes
                .Include(r => r.ContaFinanceira)
                .Include(r => r.TipoDespesa)
                .Include(r => r.NaturezaDespesa)
                .Include(r => r.Categoria);
        }

        private IQueryable<DespesaRecorrenteOcorrencia> OcorrenciasComRelacionamentos()
        {
            return _context.DespesasRecorrentesOcorrencias
                .Include(o => o.DespesaRecorrente)
                    .ThenInclude(r => r.ContaFinanceira)
                .Include(o => o.DespesaRecorrente)
                    .ThenInclude(r => r.TipoDespesa)
                .Include(o => o.DespesaRecorrente)
                    .ThenInclude(r => r.NaturezaDespesa)
                .Include(o => o.DespesaRecorrente)
                    .ThenInclude(r => r.Categoria);
        }
    }
}
