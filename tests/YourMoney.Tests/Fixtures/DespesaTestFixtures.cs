using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Application.Services;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;

namespace YourMoney.Tests.Fixtures
{
    public static class DespesaTestFixtures
    {
        public static readonly Guid ContaId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public static readonly Guid CategoriaId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        public static ParcelamentoDespesaRequest Request(
            decimal valorTotal = 100m,
            int quantidadeParcelas = 3,
            DateTime? dataInicial = null)
        {
            return new ParcelamentoDespesaRequest
            {
                Descricao = "Notebook",
                ValorTotal = valorTotal,
                QuantidadeParcelas = quantidadeParcelas,
                DataInicial = dataInicial ?? new DateTime(2026, 5, 26),
                IdContaFinanceira = ContaId,
                IdCategoria = CategoriaId
            };
        }

        public static CriarDespesaRequest CriarRequest(
            string? descricao = "Mercado",
            decimal valor = 120.35m,
            DateTime? data = null,
            DateTime? mesReferencia = null,
            Guid? idContaFinanceira = null,
            Guid? idCategoria = null)
        {
            return new CriarDespesaRequest
            {
                Descricao = descricao,
                Valor = valor,
                Data = data ?? new DateTime(2026, 5, 30),
                MesReferencia = mesReferencia ?? new DateTime(2026, 5, 1),
                IdContaFinanceira = idContaFinanceira ?? ContaId,
                IdCategoria = idCategoria ?? CategoriaId
            };
        }

        public static DespesaService CreateService(
            InMemoryDespesaRepository? despesaRepository = null,
            InMemoryReceitaRepository? receitaRepository = null,
            bool contaExists = true,
            bool categoriaExists = true)
        {
            return new DespesaService(
                despesaRepository ?? new InMemoryDespesaRepository(),
                receitaRepository ?? new InMemoryReceitaRepository(),
                new ContaFinanceiraRepositoryStub(contaExists),
                new CategoriaRepositoryStub(categoriaExists),
                new FakeCurrentUserService());
        }
    }

    public class FakeCurrentUserService : ICurrentUserService
    {
        public string UserId { get; set; } = "test-user";
        public bool IsAuthenticated => true;
    }

    public class InMemoryDespesaRepository : IDespesaRepository
    {
        public List<Despesa> Despesas { get; } = new();
        public bool FailBatchInsert { get; set; }

        public Task<Despesa> GetByIdAsync(Guid id)
        {
            var despesa = Despesas.FirstOrDefault(d => d.Id == id)
                ?? throw new InvalidOperationException("Despesa não encontrada.");
            return Task.FromResult(despesa);
        }

        public Task<Despesa> GetByIdAsync(Guid id, string usuarioId)
        {
            var despesa = Despesas.FirstOrDefault(d => d.Id == id && d.UsuarioId == usuarioId)
                ?? throw new InvalidOperationException("Despesa não encontrada.");
            return Task.FromResult(despesa);
        }

        public Task<List<Despesa>> GetAllAsync() => Task.FromResult(Despesas.ToList());
        public Task<List<Despesa>> GetAllAsync(string usuarioId) => Task.FromResult(Despesas.Where(d => d.UsuarioId == usuarioId).ToList());

        public Task<List<Despesa>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim)
        {
            return Task.FromResult(Despesas
                .Where(d => d.Data >= dataInicio && d.Data <= dataFim)
                .ToList());
        }

        public Task<List<Despesa>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim, string usuarioId)
        {
            return Task.FromResult(Despesas
                .Where(d => d.UsuarioId == usuarioId && d.Data >= dataInicio && d.Data <= dataFim)
                .ToList());
        }

        public Task<List<Despesa>> GetByMesAnoAsync(int mes, int ano)
        {
            return Task.FromResult(Despesas
                .Where(d => d.Data.Month == mes && d.Data.Year == ano)
                .ToList());
        }

        public Task<List<Despesa>> GetByMesAnoAsync(int mes, int ano, string usuarioId)
        {
            return Task.FromResult(Despesas
                .Where(d => d.UsuarioId == usuarioId && d.Data.Month == mes && d.Data.Year == ano)
                .ToList());
        }

        public Task<List<Despesa>> ObterPorMesAnoAsync(int mes, int ano, Guid? idContaFinanceira)
        {
            return Task.FromResult(Despesas
                .Where(d => d.Data.Month == mes && d.Data.Year == ano)
                .Where(d => idContaFinanceira == null || d.IdContaFinanceira == idContaFinanceira)
                .ToList());
        }

        public Task<List<Despesa>> ObterPorMesAnoAsync(int mes, int ano, string usuarioId, Guid? idContaFinanceira)
        {
            return Task.FromResult(Despesas
                .Where(d => d.UsuarioId == usuarioId && d.Data.Month == mes && d.Data.Year == ano)
                .Where(d => idContaFinanceira == null || d.IdContaFinanceira == idContaFinanceira)
                .ToList());
        }

        public Task<bool> ExisteAsync(Guid id, string usuarioId)
        {
            return Task.FromResult(Despesas.Any(d => d.Id == id && d.UsuarioId == usuarioId));
        }

        public Task<List<Despesa>> GetByCategoriaAsync(Guid categoriaId)
        {
            return Task.FromResult(Despesas.Where(d => d.IdCategoria == categoriaId).ToList());
        }

        public Task<List<Despesa>> GetByCategoriaAsync(Guid categoriaId, string usuarioId)
        {
            return Task.FromResult(Despesas.Where(d => d.UsuarioId == usuarioId && d.IdCategoria == categoriaId).ToList());
        }

        public Task AdicionarAsync(Despesa despesa)
        {
            Despesas.Add(despesa);
            return Task.CompletedTask;
        }

        public Task AdicionarEmLoteAsync(IReadOnlyCollection<Despesa> despesas)
        {
            if (FailBatchInsert)
                throw new InvalidOperationException("Falha simulada ao persistir parcelas.");

            Despesas.AddRange(despesas);
            return Task.CompletedTask;
        }

        public Task AtualizarAsync(Despesa despesa)
        {
            return Task.CompletedTask;
        }

        public Task RemoverAsync(Guid id)
        {
            Despesas.RemoveAll(d => d.Id == id);
            return Task.CompletedTask;
        }

        public Task RemoverAsync(Guid id, string usuarioId)
        {
            Despesas.RemoveAll(d => d.Id == id && d.UsuarioId == usuarioId);
            return Task.CompletedTask;
        }

        public Task<List<Despesa>> ListarAsync() => Task.FromResult(Despesas.ToList());
        public Task<List<Despesa>> ListarAsync(string usuarioId) => Task.FromResult(Despesas.Where(d => d.UsuarioId == usuarioId).ToList());
    }

    public class ContaFinanceiraRepositoryStub : IContaFinanceiraRepository
    {
        private readonly bool _exists;

        public ContaFinanceiraRepositoryStub(bool exists)
        {
            _exists = exists;
        }

        public Task<ContaFinanceira> GetByIdAsync(Guid id) => throw new NotImplementedException();
        public Task<ContaFinanceira> GetByIdAsync(Guid id, string usuarioId) => throw new NotImplementedException();
        public Task AdicionarAsync(ContaFinanceira contaFinanceira) => throw new NotImplementedException();
        public Task AtualizarAsync(ContaFinanceira contaFinanceira) => throw new NotImplementedException();
        public Task RemoverAsync(Guid id) => throw new NotImplementedException();
        public Task RemoverAsync(Guid id, string usuarioId) => throw new NotImplementedException();
        public Task<List<ContaFinanceira>> ListarAsync() => throw new NotImplementedException();
        public Task<List<ContaFinanceira>> ListarAsync(string usuarioId) => throw new NotImplementedException();
        public Task<bool> ExisteAsync(Guid id) => Task.FromResult(_exists && id != Guid.Empty);
        public Task<bool> ExisteAsync(Guid id, string usuarioId) => Task.FromResult(_exists && id != Guid.Empty && !string.IsNullOrWhiteSpace(usuarioId));
    }

    public class CategoriaRepositoryStub : ICategoriaRepository
    {
        private readonly bool _exists;

        public CategoriaRepositoryStub(bool exists)
        {
            _exists = exists;
        }

        public Task<Categoria> GetByIdAsync(Guid id) => throw new NotImplementedException();
        public Task<Categoria> GetByIdAsync(Guid id, string usuarioId) => throw new NotImplementedException();
        public Task AdicionarAsync(Categoria categoria) => throw new NotImplementedException();
        public Task AtualizarAsync(Categoria categoria) => throw new NotImplementedException();
        public Task RemoverAsync(Guid id) => throw new NotImplementedException();
        public Task RemoverAsync(Guid id, string usuarioId) => throw new NotImplementedException();
        public Task<List<Categoria>> GetAllAsync() => throw new NotImplementedException();
        public Task<List<Categoria>> GetAllAsync(string usuarioId) => throw new NotImplementedException();
        public Task<bool> ExisteAsync(Guid id) => Task.FromResult(_exists && id != Guid.Empty);
        public Task<bool> ExisteAsync(Guid id, string usuarioId) => Task.FromResult(_exists && id != Guid.Empty && !string.IsNullOrWhiteSpace(usuarioId));
    }

    public class FakeDespesaService : IDespesaService
    {
        public CriarDespesaResponse CriarDespesaResponse { get; set; } = new();
        public CriarDespesaRequest? LastCriarDespesaRequest { get; private set; }
        public ParcelamentoDespesaResponse ParcelamentoResponse { get; set; } = new();
        public List<DespesaDTO> QueryResponse { get; set; } = new();

        public Task AdicionarDespesaAsync(Despesa despesa) => Task.CompletedTask;
        public Task<CriarDespesaResponse> CriarDespesaAsync(CriarDespesaRequest request)
        {
            LastCriarDespesaRequest = request;
            return Task.FromResult(CriarDespesaResponse);
        }
        public Task<Despesa> GetDespesaByIdAsync(Guid id) => throw new NotImplementedException();
        public Task<DespesaDTO> ObterDtoPorIdAsync(Guid id) => Task.FromResult(QueryResponse.FirstOrDefault(d => d.Id == id) ?? new DespesaDTO { Id = id });
        public Task RemoverDespesaAsync(Guid id) => Task.CompletedTask;
        public Task AtualizarAsync(Despesa despesa) => Task.CompletedTask;
        public Task<List<Despesa>> ListarAsync() => Task.FromResult(new List<Despesa>());
        public Task<List<DespesaDTO>> ObterPorMesAnoAsync(int mes, int ano, Guid? idContaFinanceira = null) => Task.FromResult(QueryResponse);
        public Task<ParcelamentoDespesaResponse> CriarParcelamentoAsync(ParcelamentoDespesaRequest request) => Task.FromResult(ParcelamentoResponse);
    }
}
