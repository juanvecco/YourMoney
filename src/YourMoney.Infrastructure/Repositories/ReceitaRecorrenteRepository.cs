using Microsoft.EntityFrameworkCore;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;
using YourMoney.Infrastructure.Persistence;

namespace YourMoney.Infrastructure.Repositories
{
    public class ReceitaRecorrenteRepository : IReceitaRecorrenteRepository
    {
        private readonly AppDbContext _context;

        public ReceitaRecorrenteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AdicionarAsync(ReceitaRecorrente recorrencia)
        {
            await _context.ReceitasRecorrentes.AddAsync(recorrencia);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(ReceitaRecorrente recorrencia)
        {
            _context.ReceitasRecorrentes.Update(recorrencia);
            await _context.SaveChangesAsync();
        }

        public Task<ReceitaRecorrente?> ObterPorIdAsync(Guid id, string usuarioId)
        {
            return _context.ReceitasRecorrentes
                .Include(r => r.ContaFinanceira)
                .FirstOrDefaultAsync(r => r.Id == id && r.UsuarioId == usuarioId);
        }

        public async Task<List<ReceitaRecorrente>> ListarAsync(string usuarioId, bool? ativas = null)
        {
            var query = _context.ReceitasRecorrentes
                .Include(r => r.ContaFinanceira)
                .Where(r => r.UsuarioId == usuarioId);

            if (ativas.HasValue)
                query = query.Where(r => r.Ativa == ativas.Value);

            return await query.OrderBy(r => r.Descricao).ToListAsync();
        }

        public Task<List<ReceitaRecorrente>> ListarElegiveisParaMesAsync(string usuarioId, DateTime mesReferencia)
        {
            var mes = ReceitaRecorrente.NormalizarMesReferencia(mesReferencia);
            var fimMes = new DateTime(mes.Year, mes.Month, DateTime.DaysInMonth(mes.Year, mes.Month));

            return _context.ReceitasRecorrentes
                .Include(r => r.ContaFinanceira)
                .Where(r => r.UsuarioId == usuarioId
                    && r.Ativa
                    && r.DataInicio <= fimMes
                    && (!r.DataTermino.HasValue || r.DataTermino.Value >= mes))
                .OrderBy(r => r.Descricao)
                .ToListAsync();
        }

        public Task<List<ReceitaRecorrente>> ListarElegiveisParaReservaAsync(string usuarioId, DateTime mesReferencia)
        {
            var mes = ReceitaRecorrente.NormalizarMesReferencia(mesReferencia);
            var fimMes = new DateTime(mes.Year, mes.Month, DateTime.DaysInMonth(mes.Year, mes.Month));

            return _context.ReceitasRecorrentes
                .Include(r => r.ContaFinanceira)
                .Where(r => r.UsuarioId == usuarioId
                    && r.Ativa
                    && r.ConsideraReservaEmergencia
                    && r.DataInicio <= fimMes
                    && (!r.DataTermino.HasValue || r.DataTermino.Value >= mes))
                .OrderBy(r => r.Descricao)
                .ThenBy(r => r.Id)
                .ToListAsync();
        }

        public async Task AdicionarOcorrenciaAsync(ReceitaRecorrenteOcorrencia ocorrencia)
        {
            await _context.ReceitasRecorrentesOcorrencias.AddAsync(ocorrencia);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarOcorrenciaAsync(ReceitaRecorrenteOcorrencia ocorrencia)
        {
            _context.ReceitasRecorrentesOcorrencias.Update(ocorrencia);
            await _context.SaveChangesAsync();
        }

        public Task<ReceitaRecorrenteOcorrencia?> ObterOcorrenciaAsync(Guid recorrenciaId, DateTime mesReferencia, string usuarioId)
        {
            var mes = ReceitaRecorrente.NormalizarMesReferencia(mesReferencia);
            return _context.ReceitasRecorrentesOcorrencias
                .FirstOrDefaultAsync(o => o.UsuarioId == usuarioId
                    && o.ReceitaRecorrenteId == recorrenciaId
                    && o.MesReferencia == mes);
        }

        public Task<ReceitaRecorrenteOcorrencia?> ObterOcorrenciaPorIdAsync(Guid ocorrenciaId, string usuarioId)
        {
            return _context.ReceitasRecorrentesOcorrencias
                .Include(o => o.ReceitaRecorrente)
                    .ThenInclude(r => r.ContaFinanceira)
                .FirstOrDefaultAsync(o => o.Id == ocorrenciaId && o.UsuarioId == usuarioId);
        }

        public Task<List<ReceitaRecorrenteOcorrencia>> ListarOcorrenciasPorMesAsync(string usuarioId, DateTime mesReferencia)
        {
            var mes = ReceitaRecorrente.NormalizarMesReferencia(mesReferencia);
            return _context.ReceitasRecorrentesOcorrencias
                .Include(o => o.ReceitaRecorrente)
                    .ThenInclude(r => r.ContaFinanceira)
                .Where(o => o.UsuarioId == usuarioId && o.MesReferencia == mes)
                .OrderBy(o => o.ReceitaRecorrente.Descricao)
                .ToListAsync();
        }
    }
}
