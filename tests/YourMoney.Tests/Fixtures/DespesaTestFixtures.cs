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

        public static DespesaService CreateService(
            InMemoryDespesaRepository? despesaRepository = null,
            bool contaExists = true,
            bool categoriaExists = true)
        {
            return new DespesaService(
                despesaRepository ?? new InMemoryDespesaRepository(),
                new ContaFinanceiraRepositoryStub(contaExists),
                new CategoriaRepositoryStub(categoriaExists));
        }
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

        public Task<List<Despesa>> GetAllAsync() => Task.FromResult(Despesas.ToList());

        public Task<List<Despesa>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim)
        {
            return Task.FromResult(Despesas
                .Where(d => d.Data >= dataInicio && d.Data <= dataFim)
                .ToList());
        }

        public Task<List<Despesa>> GetByMesAnoAsync(int mes, int ano)
        {
            return Task.FromResult(Despesas
                .Where(d => d.Data.Month == mes && d.Data.Year == ano)
                .ToList());
        }

        public Task<List<Despesa>> ObterPorMesAnoAsync(int mes, int ano, Guid? idContaFinanceira)
        {
            return Task.FromResult(Despesas
                .Where(d => d.Data.Month == mes && d.Data.Year == ano)
                .Where(d => idContaFinanceira == null || d.IdContaFinanceira == idContaFinanceira)
                .ToList());
        }

        public Task<List<Despesa>> GetByCategoriaAsync(Guid categoriaId)
        {
            return Task.FromResult(Despesas.Where(d => d.IdCategoria == categoriaId).ToList());
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

        public Task<List<Despesa>> ListarAsync() => Task.FromResult(Despesas.ToList());
    }

    public class ContaFinanceiraRepositoryStub : IContaFinanceiraRepository
    {
        private readonly bool _exists;

        public ContaFinanceiraRepositoryStub(bool exists)
        {
            _exists = exists;
        }

        public Task<ContaFinanceira> GetByIdAsync(Guid id) => throw new NotImplementedException();
        public Task AdicionarAsync(ContaFinanceira contaFinanceira) => throw new NotImplementedException();
        public Task AtualizarAsync(ContaFinanceira contaFinanceira) => throw new NotImplementedException();
        public Task RemoverAsync(Guid id) => throw new NotImplementedException();
        public Task<List<ContaFinanceira>> ListarAsync() => throw new NotImplementedException();
        public Task<bool> ExisteAsync(Guid id) => Task.FromResult(_exists && id != Guid.Empty);
    }

    public class CategoriaRepositoryStub : ICategoriaRepository
    {
        private readonly bool _exists;

        public CategoriaRepositoryStub(bool exists)
        {
            _exists = exists;
        }

        public Task<Categoria> GetByIdAsync(Guid id) => throw new NotImplementedException();
        public Task AdicionarAsync(Categoria categoria) => throw new NotImplementedException();
        public Task AtualizarAsync(Categoria categoria) => throw new NotImplementedException();
        public Task RemoverAsync(Guid id) => throw new NotImplementedException();
        public Task<List<Categoria>> GetAllAsync() => throw new NotImplementedException();
        public Task<bool> ExisteAsync(Guid id) => Task.FromResult(_exists && id != Guid.Empty);
    }

    public class FakeDespesaService : IDespesaService
    {
        public ParcelamentoDespesaResponse ParcelamentoResponse { get; set; } = new();
        public List<DespesaDTO> QueryResponse { get; set; } = new();

        public Task AdicionarDespesaAsync(Despesa despesa) => Task.CompletedTask;
        public Task<Despesa> GetDespesaByIdAsync(Guid id) => throw new NotImplementedException();
        public Task RemoverDespesaAsync(Guid id) => Task.CompletedTask;
        public Task AtualizarAsync(Despesa despesa) => Task.CompletedTask;
        public Task<List<Despesa>> ListarAsync() => Task.FromResult(new List<Despesa>());
        public Task<List<DespesaDTO>> ObterPorMesAnoAsync(int mes, int ano, Guid? idContaFinanceira = null) => Task.FromResult(QueryResponse);
        public Task<ParcelamentoDespesaResponse> CriarParcelamentoAsync(ParcelamentoDespesaRequest request) => Task.FromResult(ParcelamentoResponse);
    }
}
