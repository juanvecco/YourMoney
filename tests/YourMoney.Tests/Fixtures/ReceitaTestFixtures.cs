using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Application.Services;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;

namespace YourMoney.Tests.Fixtures
{
    public static class ReceitaTestFixtures
    {
        public static CriarReceitaRequest CriarRequest(
            string? descricao = "Salário",
            decimal valor = 5250.755m,
            DateTime? data = null,
            DateTime? mesReferencia = null)
        {
            return new CriarReceitaRequest
            {
                Descricao = descricao,
                Valor = valor,
                Data = data ?? new DateTime(2026, 6, 5, 18, 30, 0),
                MesReferencia = mesReferencia ?? new DateTime(2026, 5, 20)
            };
        }

        public static ReceitaService CreateService(
            InMemoryReceitaRepository? repository = null,
            FakeCurrentUserService? currentUser = null)
        {
            return new ReceitaService(
                repository ?? new InMemoryReceitaRepository(),
                currentUser ?? new FakeCurrentUserService());
        }
    }

    public class InMemoryReceitaRepository : IReceitaRepository
    {
        public List<Receita> Receitas { get; } = new();

        public Task<Receita> GetByIdAsync(Guid id) =>
            Task.FromResult(Receitas.First(r => r.Id == id));

        public Task<Receita> GetByIdAsync(Guid id, string usuarioId) =>
            Task.FromResult(Receitas.First(r => r.Id == id && r.UsuarioId == usuarioId));

        public Task<List<Receita>> GetAllAsync() => Task.FromResult(Receitas.ToList());
        public Task<List<Receita>> GetAllAsync(string usuarioId) => ListarAsync(usuarioId);

        public Task<List<Receita>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim) =>
            Task.FromResult(Receitas.Where(r => r.Data >= dataInicio && r.Data <= dataFim).ToList());

        public Task<List<Receita>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim, string usuarioId) =>
            Task.FromResult(Receitas.Where(r => r.UsuarioId == usuarioId && r.Data >= dataInicio && r.Data <= dataFim).ToList());

        public Task<List<Receita>> GetByMesAnoAsync(int mes, int ano) =>
            Task.FromResult(FilterByReference(Receitas, mes, ano).ToList());

        public Task<List<Receita>> GetByMesAnoAsync(int mes, int ano, string usuarioId) =>
            Task.FromResult(FilterByReference(Receitas.Where(r => r.UsuarioId == usuarioId), mes, ano).ToList());

        public Task<List<Receita>> ObterPorMesAnoAsync(int mes, int ano) => GetByMesAnoAsync(mes, ano);
        public Task<List<Receita>> ObterPorMesAnoAsync(int mes, int ano, string usuarioId) => GetByMesAnoAsync(mes, ano, usuarioId);
        public Task<List<Receita>> GetByCategoriaAsync(Guid categoriaId) => Task.FromResult(new List<Receita>());
        public Task<List<Receita>> GetPendentesAsync() => Task.FromResult(new List<Receita>());

        public Task AdicionarAsync(Receita receita)
        {
            Receitas.Add(receita);
            return Task.CompletedTask;
        }

        public Task AtualizarAsync(Receita receita) => Task.CompletedTask;

        public Task RemoverAsync(Guid id)
        {
            Receitas.RemoveAll(r => r.Id == id);
            return Task.CompletedTask;
        }

        public Task RemoverAsync(Guid id, string usuarioId)
        {
            Receitas.RemoveAll(r => r.Id == id && r.UsuarioId == usuarioId);
            return Task.CompletedTask;
        }

        public async Task<decimal> GetTotalByMesAnoAsync(int mes, int ano) =>
            (await GetByMesAnoAsync(mes, ano)).Sum(r => r.Valor);

        public async Task<decimal> GetTotalByMesAnoAsync(int mes, int ano, string usuarioId) =>
            (await GetByMesAnoAsync(mes, ano, usuarioId)).Sum(r => r.Valor);

        public Task<List<Receita>> ListarAsync() => Task.FromResult(Receitas.ToList());
        public Task<List<Receita>> ListarAsync(string usuarioId) =>
            Task.FromResult(Receitas.Where(r => r.UsuarioId == usuarioId).ToList());

        private static IEnumerable<Receita> FilterByReference(IEnumerable<Receita> receitas, int mes, int ano)
        {
            return receitas.Where(r =>
                (r.MesReferencia.HasValue
                    && r.MesReferencia.Value.Month == mes
                    && r.MesReferencia.Value.Year == ano)
                || (!r.MesReferencia.HasValue && r.Data.Month == mes && r.Data.Year == ano));
        }
    }

    public class FakeReceitaService : IReceitaService
    {
        public CriarReceitaResponse CreateResponse { get; set; } = new();
        public CriarReceitaRequest? LastRequest { get; private set; }
        public Exception? CreateException { get; set; }

        public Task AdicionarReceitaAsync(Receita receita) => Task.CompletedTask;

        public Task<CriarReceitaResponse> CriarReceitaAsync(CriarReceitaRequest request)
        {
            LastRequest = request;
            if (CreateException != null)
                throw CreateException;
            return Task.FromResult(CreateResponse);
        }

        public Task<Receita> GetReceitaByIdAsync(Guid id) => throw new NotImplementedException();
        public Task RemoverReceitaAsync(Guid id) => Task.CompletedTask;
        public Task AtualizarAsync(Receita receita) => Task.CompletedTask;
        public Task<List<ReceitaDTO>> ListarAsync() => Task.FromResult(new List<ReceitaDTO>());
        public Task<List<ReceitaDTO>> ObterPorMesAnoAsync(int mes, int ano) => Task.FromResult(new List<ReceitaDTO>());
    }
}
