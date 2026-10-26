#nullable enable
using System.Data;
using Microsoft.EntityFrameworkCore;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;
using YourMoney.Infrastructure.Persistence;

namespace YourMoney.Infrastructure.Repositories;

public sealed class GrupoObjetivoInvestimentoRepository : IGrupoObjetivoInvestimentoRepository
{
    private readonly AppDbContext _context;

    public GrupoObjetivoInvestimentoRepository(AppDbContext context) => _context = context;

    public Task<List<GrupoObjetivoInvestimento>> ListarAsync(string usuarioId) =>
        _context.GruposObjetivoInvestimento
            .AsNoTracking()
            .Where(g => g.UsuarioId == usuarioId)
            .OrderBy(g => g.Nome)
            .ThenBy(g => g.Id)
            .ToListAsync();

    public Task<GrupoObjetivoInvestimento?> ObterAsync(Guid id, string usuarioId, bool rastrear = true)
    {
        var query = _context.GruposObjetivoInvestimento.Where(g => g.Id == id && g.UsuarioId == usuarioId);
        return (rastrear ? query : query.AsNoTracking()).FirstOrDefaultAsync();
    }

    public Task<int> ContarInvestimentosAsync(Guid id, string usuarioId) =>
        _context.Investimentos.CountAsync(i => i.UsuarioId == usuarioId && i.GrupoObjetivoInvestimentoId == id);

    public async Task AdicionarAsync(GrupoObjetivoInvestimento grupo)
    {
        try
        {
            _context.GruposObjetivoInvestimento.Add(grupo);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new GrupoObjetivoConflitoException("grupo_nome_duplicado", "Já existe um objetivo financeiro com esse nome.");
        }
    }

    public async Task AtualizarAsync(GrupoObjetivoInvestimento grupo, byte[] versaoEsperada)
    {
        _context.Entry(grupo).Property(g => g.Versao).OriginalValue = versaoEsperada;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new GrupoObjetivoConflitoException("grupo_desatualizado", "O objetivo foi alterado. Atualize os dados e tente novamente.");
        }
        catch (DbUpdateException)
        {
            throw new GrupoObjetivoConflitoException("grupo_nome_duplicado", "Já existe um objetivo financeiro com esse nome.");
        }
    }

    public async Task ExcluirAsync(ExcluirGrupoObjetivoCommand command)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            var origem = await _context.GruposObjetivoInvestimento
                .FirstOrDefaultAsync(g => g.Id == command.GrupoOrigemId && g.UsuarioId == command.UsuarioId)
                ?? throw new KeyNotFoundException("Objetivo financeiro não encontrado.");

            ValidarVersao(origem.Versao, command.VersaoOrigem);
            var quantidade = await _context.Investimentos.CountAsync(i =>
                i.UsuarioId == command.UsuarioId && i.GrupoObjetivoInvestimentoId == origem.Id);

            if (command.AcaoVinculos == AcaoVinculosGrupoObjetivo.ExigirVazio && quantidade > 0)
                throw new GrupoObjetivoConflitoException("grupo_em_uso", "O objetivo possui investimentos associados.");

            if (command.AcaoVinculos == AcaoVinculosGrupoObjetivo.Desassociar && quantidade > 0)
            {
                await _context.Investimentos
                    .Where(i => i.UsuarioId == command.UsuarioId && i.GrupoObjetivoInvestimentoId == origem.Id)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(i => i.GrupoObjetivoInvestimentoId, (Guid?)null));
            }

            if (command.AcaoVinculos == AcaoVinculosGrupoObjetivo.Transferir)
            {
                if (!command.GrupoDestinoId.HasValue || command.GrupoDestinoId == origem.Id)
                    throw new ArgumentException("Selecione outro objetivo financeiro como destino.");

                var destino = await _context.GruposObjetivoInvestimento.FirstOrDefaultAsync(g =>
                    g.Id == command.GrupoDestinoId.Value && g.UsuarioId == command.UsuarioId)
                    ?? throw new KeyNotFoundException("Objetivo financeiro de destino não encontrado.");
                ValidarVersao(destino.Versao, command.VersaoDestino ?? Array.Empty<byte>());

                await _context.Investimentos
                    .Where(i => i.UsuarioId == command.UsuarioId && i.GrupoObjetivoInvestimentoId == origem.Id)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(i => i.GrupoObjetivoInvestimentoId, destino.Id));
            }

            _context.Entry(origem).Property(g => g.Versao).OriginalValue = command.VersaoOrigem;
            _context.GruposObjetivoInvestimento.Remove(origem);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            throw new GrupoObjetivoConflitoException("grupo_desatualizado", "O objetivo foi alterado. Atualize os dados e tente novamente.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static void ValidarVersao(byte[] atual, byte[] esperada)
    {
        if (!atual.SequenceEqual(esperada))
            throw new GrupoObjetivoConflitoException("grupo_desatualizado", "O objetivo foi alterado. Atualize os dados e tente novamente.");
    }
}
