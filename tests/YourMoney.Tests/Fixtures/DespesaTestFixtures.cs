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
        public static readonly Guid ContaLazerId = Guid.Parse("11111111-1111-1111-1111-111111111112");
        public static readonly Guid TipoEssencialId = Guid.Parse("22222222-2222-2222-2222-222222222220");
        public static readonly Guid TipoLazerId = Guid.Parse("22222222-2222-2222-2222-222222222221");
        public static readonly Guid NaturezaMoradiaId = Guid.Parse("22222222-2222-2222-2222-222222222230");
        public static readonly Guid NaturezaMercadoId = Guid.Parse("22222222-2222-2222-2222-222222222231");
        public static readonly Guid NaturezaPasseioId = Guid.Parse("22222222-2222-2222-2222-222222222232");
        public static readonly Guid CategoriaId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        public static readonly Guid CategoriaAluguelId = Guid.Parse("22222222-2222-2222-2222-222222222240");
        public static readonly Guid CategoriaCinemaId = Guid.Parse("22222222-2222-2222-2222-222222222241");

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
            bool categoriaExists = true,
            CategoriaRepositoryStub? categoriaRepository = null,
            ContaFinanceiraRepositoryStub? contaRepository = null)
        {
            return new DespesaService(
                despesaRepository ?? new InMemoryDespesaRepository(),
                receitaRepository ?? new InMemoryReceitaRepository(),
                contaRepository ?? new ContaFinanceiraRepositoryStub(contaExists),
                categoriaRepository ?? new CategoriaRepositoryStub(categoriaExists),
                new FakeCurrentUserService());
        }

        public static List<Categoria> CategoriasPadrao(string usuarioId = "test-user")
        {
            return new List<Categoria>
            {
                NovaCategoria("Essencial", TipoEssencialId, null, usuarioId),
                NovaCategoria("Lazer", TipoLazerId, null, usuarioId),
                NovaCategoria("Moradia", NaturezaMoradiaId, TipoEssencialId, usuarioId),
                NovaCategoria("Mercado", NaturezaMercadoId, TipoEssencialId, usuarioId),
                NovaCategoria("Passeio", NaturezaPasseioId, TipoLazerId, usuarioId),
                NovaCategoria("Aluguel", CategoriaAluguelId, NaturezaMoradiaId, usuarioId),
                NovaCategoria("Cinema", CategoriaCinemaId, NaturezaPasseioId, usuarioId),
                NovaCategoria("Categoria Base", CategoriaId, TipoEssencialId, usuarioId)
            };
        }

        private static Categoria NovaCategoria(string descricao, Guid id, Guid? categoriaPaiId, string usuarioId)
        {
            var categoria = new Categoria(descricao, YourMoney.Domain.Enums.TipoTransacao.Despesa, usuarioId, categoriaPaiId);
            typeof(Categoria).GetProperty(nameof(Categoria.Id))!.SetValue(categoria, id);
            return categoria;
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

        public Task<List<Despesa>> ConsultarAsync(
            int mes,
            int ano,
            string usuarioId,
            Guid? idContaFinanceira,
            IReadOnlyCollection<Guid> idsCategoria)
        {
            return Task.FromResult(Despesas
                .Where(d => d.UsuarioId == usuarioId && d.Data.Month == mes && d.Data.Year == ano)
                .Where(d => idContaFinanceira == null || d.IdContaFinanceira == idContaFinanceira)
                .Where(d => idsCategoria == null || idsCategoria.Contains(d.IdCategoria))
                .OrderByDescending(d => d.Data)
                .ThenByDescending(d => d.Id)
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
        private readonly HashSet<Guid>? _existingIds;

        public ContaFinanceiraRepositoryStub(bool exists, IEnumerable<Guid>? existingIds = null)
        {
            _exists = exists;
            _existingIds = existingIds == null ? null : new HashSet<Guid>(existingIds);
        }

        public Task<ContaFinanceira> GetByIdAsync(Guid id) => Task.FromResult(CriarConta(id, "Conta Principal", "test-user"));
        public Task<ContaFinanceira> GetByIdAsync(Guid id, string usuarioId)
        {
            if (!Existe(id) || string.IsNullOrWhiteSpace(usuarioId))
                throw new InvalidOperationException("Conta Financeira não encontrada.");

            return Task.FromResult(CriarConta(id, "Conta Principal", usuarioId));
        }
        public Task AdicionarAsync(ContaFinanceira contaFinanceira) => throw new NotImplementedException();
        public Task AtualizarAsync(ContaFinanceira contaFinanceira) => throw new NotImplementedException();
        public Task RemoverAsync(Guid id) => throw new NotImplementedException();
        public Task RemoverAsync(Guid id, string usuarioId) => throw new NotImplementedException();
        public Task<List<ContaFinanceira>> ListarAsync() => throw new NotImplementedException();
        public Task<List<ContaFinanceira>> ListarAsync(string usuarioId) => throw new NotImplementedException();
        public Task<bool> ExisteAsync(Guid id) => Task.FromResult(Existe(id));
        public Task<bool> ExisteAsync(Guid id, string usuarioId) => Task.FromResult(Existe(id) && !string.IsNullOrWhiteSpace(usuarioId));

        private bool Existe(Guid id)
        {
            return _exists && id != Guid.Empty && (_existingIds == null || _existingIds.Contains(id));
        }

        private static ContaFinanceira CriarConta(Guid id, string descricao, string usuarioId)
        {
            var conta = new ContaFinanceira(descricao, usuarioId);
            typeof(ContaFinanceira).GetProperty(nameof(ContaFinanceira.Id))!.SetValue(conta, id);
            return conta;
        }
    }

    public class CategoriaRepositoryStub : ICategoriaRepository
    {
        private readonly bool _exists;
        private readonly List<Categoria> _categorias;
        private readonly bool _enforceUsuarioId;

        public CategoriaRepositoryStub(bool exists, List<Categoria>? categorias = null)
        {
            _exists = exists;
            _categorias = categorias ?? DespesaTestFixtures.CategoriasPadrao();
            _enforceUsuarioId = categorias != null;
        }

        public Task<Categoria> GetByIdAsync(Guid id) => Task.FromResult(_categorias.First(c => c.Id == id));
        public Task<Categoria> GetByIdAsync(Guid id, string usuarioId) => Task.FromResult(_categorias.First(c => CategoriaPertenceAoUsuario(c, id, usuarioId)));
        public Task AdicionarAsync(Categoria categoria) => throw new NotImplementedException();
        public Task AtualizarAsync(Categoria categoria) => throw new NotImplementedException();
        public Task RemoverAsync(Guid id) => throw new NotImplementedException();
        public Task RemoverAsync(Guid id, string usuarioId) => throw new NotImplementedException();
        public Task<List<Categoria>> GetAllAsync() => Task.FromResult(_categorias.ToList());
        public Task<List<Categoria>> GetAllAsync(string usuarioId) => Task.FromResult(_categorias.Where(c => !_enforceUsuarioId || c.UsuarioId == usuarioId).ToList());
        public Task<bool> ExisteAsync(Guid id) => Task.FromResult(_exists && _categorias.Any(c => c.Id == id));
        public Task<bool> ExisteAsync(Guid id, string usuarioId) => Task.FromResult(_exists && _categorias.Any(c => CategoriaPertenceAoUsuario(c, id, usuarioId)));

        private bool CategoriaPertenceAoUsuario(Categoria categoria, Guid id, string usuarioId)
        {
            return categoria.Id == id && (!_enforceUsuarioId || categoria.UsuarioId == usuarioId);
        }
    }

    public class FakeDespesaService : IDespesaService
    {
        public CriarDespesaResponse CriarDespesaResponse { get; set; } = new();
        public CriarDespesaRequest? LastCriarDespesaRequest { get; private set; }
        public ConsultaDespesasRequest? LastConsultaDespesasRequest { get; private set; }
        public ParcelamentoDespesaResponse ParcelamentoResponse { get; set; } = new();
        public List<DespesaDTO> QueryResponse { get; set; } = new();
        public ConsultaDespesasResponse ConsultaResponse { get; set; } = new();
        public Exception? ConsultaException { get; set; }

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
        public Task<ConsultaDespesasResponse> ConsultarDespesasAsync(ConsultaDespesasRequest request)
        {
            LastConsultaDespesasRequest = request;
            if (ConsultaException != null)
                throw ConsultaException;
            return Task.FromResult(ConsultaResponse);
        }
        public Task<ParcelamentoDespesaResponse> CriarParcelamentoAsync(ParcelamentoDespesaRequest request) => Task.FromResult(ParcelamentoResponse);
    }
}
