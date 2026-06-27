using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Application.Services;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;

namespace YourMoney.Tests.Fixtures
{
    public static class MetaMensalTestFixtures
    {
        public static CriarMetaMensalDTO CriarRequest(
            string? nome = "Investimento",
            decimal percentualReceita = 25m,
            DateTime? mesReferencia = null)
        {
            return new CriarMetaMensalDTO
            {
                Nome = nome,
                PercentualReceita = percentualReceita,
                MesReferencia = mesReferencia ?? new DateTime(2026, 6, 15)
            };
        }

        public static MetaMensalService CreateService(
            InMemoryMetaMensalRepository? metaRepository = null,
            InMemoryReceitaRepository? receitaRepository = null,
            InMemoryDespesaRepository? despesaRepository = null,
            FakeCurrentUserService? currentUser = null)
        {
            return new MetaMensalService(
                metaRepository ?? new InMemoryMetaMensalRepository(),
                receitaRepository ?? new InMemoryReceitaRepository(),
                despesaRepository ?? new InMemoryDespesaRepository(),
                currentUser ?? new FakeCurrentUserService());
        }
    }

    public class InMemoryMetaMensalRepository : IMetaMensalRepository
    {
        public List<MetaMensal> Metas { get; } = new();

        public Task<MetaMensal?> GetByIdAsync(Guid id, string usuarioId)
        {
            return Task.FromResult(Metas.FirstOrDefault(meta => meta.Id == id && meta.UsuarioId == usuarioId));
        }

        public Task<List<MetaMensal>> ObterPorMesAnoAsync(int mes, int ano, string usuarioId)
        {
            return Task.FromResult(Metas
                .Where(meta => meta.UsuarioId == usuarioId
                    && meta.MesReferencia.Month == mes
                    && meta.MesReferencia.Year == ano)
                .OrderBy(meta => meta.Nome)
                .ToList());
        }

        public Task AdicionarAsync(MetaMensal meta)
        {
            Metas.Add(meta);
            return Task.CompletedTask;
        }

        public Task AtualizarAsync(MetaMensal meta)
        {
            return Task.CompletedTask;
        }

        public Task RemoverAsync(Guid id, string usuarioId)
        {
            Metas.RemoveAll(meta => meta.Id == id && meta.UsuarioId == usuarioId);
            return Task.CompletedTask;
        }
    }

    public class FakeMetaMensalService : IMetaMensalService
    {
        public MetasMensaisResumoDTO SummaryResponse { get; set; } = new();
        public MetaMensalDTO MetaResponse { get; set; } = new();
        public Exception? Exception { get; set; }
        public CriarMetaMensalDTO? LastCreateRequest { get; private set; }
        public AtualizarMetaMensalDTO? LastUpdateRequest { get; private set; }
        public Guid? LastDeletedId { get; private set; }

        public Task<MetasMensaisResumoDTO> ObterResumoAsync(int? mes = null, int? ano = null)
        {
            if (Exception != null) throw Exception;
            return Task.FromResult(SummaryResponse);
        }

        public Task<MetaMensalDTO> CriarAsync(CriarMetaMensalDTO request)
        {
            LastCreateRequest = request;
            if (Exception != null) throw Exception;
            return Task.FromResult(MetaResponse);
        }

        public Task<MetaMensalDTO> AtualizarAsync(Guid id, AtualizarMetaMensalDTO request)
        {
            LastUpdateRequest = request;
            if (Exception != null) throw Exception;
            return Task.FromResult(MetaResponse);
        }

        public Task RemoverAsync(Guid id)
        {
            LastDeletedId = id;
            if (Exception != null) throw Exception;
            return Task.CompletedTask;
        }
    }
}
