using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Application.Services;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;

namespace YourMoney.Tests.Fixtures
{
    public static class InvestimentoTestFixtures
    {
        public static CriarInvestimentoRequest CriarRequest(
            string? nome = "Tesouro Selic",
            string? descricao = "Reserva",
            string? tipo = "Renda fixa",
            decimal quantidade = 2.505m,
            decimal precoMedio = 153.424m,
            decimal valorAtual = 383.555m,
            DateTime? dataInvestimento = null,
            DateTime? mesReferencia = null,
            Guid? receitaRecorrenteId = null,
            Guid? operacaoId = null)
        {
            return new CriarInvestimentoRequest
            {
                Nome = nome,
                Descricao = descricao,
                Tipo = tipo,
                Quantidade = quantidade,
                PrecoMedio = precoMedio,
                ValorAtual = valorAtual,
                DataInvestimento = dataInvestimento ?? new DateTime(2026, 6, 9, 18, 30, 0),
                MesReferencia = mesReferencia ?? new DateTime(2026, 5, 20, 12, 0, 0),
                ReceitaRecorrenteId = receitaRecorrenteId,
                OperacaoId = operacaoId ?? Guid.NewGuid()
            };
        }

        public static InvestimentoService CreateService(
            InMemoryInvestimentoRepository? repository = null,
            InMemoryReceitaInvestimentoRepository? receitaRepository = null,
            FakeCurrentUserService? currentUser = null)
        {
            return new InvestimentoService(
                repository ?? new InMemoryInvestimentoRepository(),
                receitaRepository ?? new InMemoryReceitaInvestimentoRepository(),
                currentUser ?? new FakeCurrentUserService());
        }
    }

    public class InMemoryInvestimentoRepository : IInvestimentoRepository
    {
        public List<Investimento> Investimentos { get; } = new();

        public Task<Investimento> GetByIdAsync(Guid id) => Task.FromResult(Investimentos.FirstOrDefault(i => i.Id == id)!);
        public Task<Investimento> GetByIdAsync(Guid id, string usuarioId) => Task.FromResult(Investimentos.FirstOrDefault(i => i.Id == id && i.UsuarioId == usuarioId)!);
        public Task<List<Investimento>> GetAllAsync() => Task.FromResult(Investimentos.ToList());
        public Task<List<Investimento>> GetAllAsync(string usuarioId) => ListarAsync(usuarioId);
        public Task<List<Investimento>> GetAtivosAsync() => Task.FromResult(Investimentos.Where(i => i.Ativo).ToList());
        public Task<List<Investimento>> GetAtivosAsync(string usuarioId) => Task.FromResult(Investimentos.Where(i => i.UsuarioId == usuarioId && i.Ativo).ToList());
        public Task<List<Investimento>> GetByTipoAsync(string tipo) => Task.FromResult(Investimentos.Where(i => i.Tipo == tipo).ToList());
        public Task<List<Investimento>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim) => Task.FromResult(Investimentos.Where(i => i.DataInvestimento >= dataInicio && i.DataInvestimento <= dataFim).ToList());
        public Task<List<Investimento>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim, string usuarioId) => Task.FromResult(Investimentos.Where(i => i.UsuarioId == usuarioId && i.DataInvestimento >= dataInicio && i.DataInvestimento <= dataFim).ToList());
        public Task AdicionarAsync(Investimento investimento) { Investimentos.Add(investimento); return Task.CompletedTask; }
        public Task AtualizarAsync(Investimento investimento) => Task.CompletedTask;
        public Task RemoverAsync(Guid id) { Investimentos.RemoveAll(i => i.Id == id); return Task.CompletedTask; }
        public Task RemoverAsync(Guid id, string usuarioId) { Investimentos.RemoveAll(i => i.Id == id && i.UsuarioId == usuarioId); return Task.CompletedTask; }
        public Task<decimal> GetTotalInvestidoAsync() => Task.FromResult(Investimentos.Sum(i => i.ValorAtual));
        public Task<decimal> GetTotalInvestidoAsync(string usuarioId) => Task.FromResult(Investimentos.Where(i => i.UsuarioId == usuarioId).Sum(i => i.ValorAtual));
        public Task<decimal> GetTotalAtualAsync() => GetTotalInvestidoAsync();
        public Task<decimal> GetTotalAtualAsync(string usuarioId) => GetTotalInvestidoAsync(usuarioId);
        public Task<List<Investimento>> ObterPorMesAnoAsync(int mes, int ano) => Task.FromResult(Investimentos.Where(i => PertenceAoPeriodo(i, mes, ano)).ToList());
        public Task<List<Investimento>> ObterPorMesAnoAsync(int mes, int ano, string usuarioId) => Task.FromResult(Investimentos.Where(i => i.UsuarioId == usuarioId && PertenceAoPeriodo(i, mes, ano)).ToList());
        public Task<List<Investimento>> ListarAsync() => Task.FromResult(Investimentos.ToList());
        public Task<List<Investimento>> ListarAsync(string usuarioId) => Task.FromResult(Investimentos.Where(i => i.UsuarioId == usuarioId).ToList());
        public Task<List<Investimento>> ListarConsolidadoAsync(string usuarioId) => Task.FromResult(Investimentos.Where(i => i.UsuarioId == usuarioId).OrderByDescending(i => i.DataInvestimento).ThenByDescending(i => i.Id).ToList());
        public Task<Investimento?> ObterPorOperacaoIdAsync(Guid operacaoId, string usuarioId) => Task.FromResult(Investimentos.FirstOrDefault(i => i.UsuarioId == usuarioId && i.OperacaoId == operacaoId));
        public Task<Investimento> AdicionarIdempotenteAsync(Investimento investimento)
        {
            var existente = investimento.OperacaoId.HasValue
                ? Investimentos.FirstOrDefault(i => i.UsuarioId == investimento.UsuarioId && i.OperacaoId == investimento.OperacaoId)
                : null;
            if (existente != null) return Task.FromResult(existente);
            Investimentos.Add(investimento);
            return Task.FromResult(investimento);
        }

        private static bool PertenceAoPeriodo(Investimento investimento, int mes, int ano)
        {
            var referencia = investimento.MesReferencia ?? investimento.DataInvestimento;
            return referencia.Month == mes && referencia.Year == ano;
        }
    }

    public class InMemoryReceitaInvestimentoRepository : IReceitaRecorrenteRepository
    {
        public List<ReceitaRecorrente> Recorrencias { get; } = new();
        public Task<ReceitaRecorrente?> ObterPorIdAsync(Guid id, string usuarioId) => Task.FromResult(Recorrencias.FirstOrDefault(r => r.Id == id && r.UsuarioId == usuarioId));
        public Task<List<ReceitaRecorrente>> ListarReservasSalariaisAtivasAsync(string usuarioId, DateTime mes) => Task.FromResult(Recorrencias.Where(r => r.UsuarioId == usuarioId && r.ConsideraReservaEmergencia && r.EstaElegivelParaMes(mes)).ToList());
        public Task<List<ReceitaRecorrente>> ListarElegiveisParaInvestimentoAsync(string usuarioId, DateTime mes) => Task.FromResult(Recorrencias.Where(r => r.UsuarioId == usuarioId && r.EstaElegivelParaMes(mes)).ToList());
        public Task AdicionarAsync(ReceitaRecorrente recorrencia) { Recorrencias.Add(recorrencia); return Task.CompletedTask; }
        public Task AtualizarAsync(ReceitaRecorrente recorrencia) => Task.CompletedTask;
        public Task<List<ReceitaRecorrente>> ListarAsync(string usuarioId, bool? ativas = null) => Task.FromResult(Recorrencias.Where(r => r.UsuarioId == usuarioId).ToList());
        public Task<List<ReceitaRecorrente>> ListarElegiveisParaMesAsync(string usuarioId, DateTime mes) => ListarElegiveisParaInvestimentoAsync(usuarioId, mes);
        public Task<List<ReceitaRecorrente>> ListarElegiveisParaReservaAsync(string usuarioId, DateTime mes) => ListarReservasSalariaisAtivasAsync(usuarioId, mes);
        public Task AdicionarOcorrenciaAsync(ReceitaRecorrenteOcorrencia ocorrencia) => Task.CompletedTask;
        public Task AtualizarOcorrenciaAsync(ReceitaRecorrenteOcorrencia ocorrencia) => Task.CompletedTask;
        public Task<ReceitaRecorrenteOcorrencia?> ObterOcorrenciaAsync(Guid recorrenciaId, DateTime mesReferencia, string usuarioId) => Task.FromResult<ReceitaRecorrenteOcorrencia?>(null);
        public Task<ReceitaRecorrenteOcorrencia?> ObterOcorrenciaPorIdAsync(Guid ocorrenciaId, string usuarioId) => Task.FromResult<ReceitaRecorrenteOcorrencia?>(null);
        public Task<List<ReceitaRecorrenteOcorrencia>> ListarOcorrenciasPorMesAsync(string usuarioId, DateTime mesReferencia) => Task.FromResult(new List<ReceitaRecorrenteOcorrencia>());
    }

    public class FakeInvestimentoService : IInvestimentoService
    {
        public CriarInvestimentoResponse CreateResponse { get; set; } = new();
        public CriarInvestimentoRequest? LastRequest { get; private set; }
        public AtualizarInvestimentoRequest? LastUpdateRequest { get; private set; }
        public Exception? CreateException { get; set; }
        public Exception? UpdateException { get; set; }
        public InvestimentoResponse UpdateResponse { get; set; } = new();
        public CarteiraInvestimentosConsolidadaResponse ConsolidatedResponse { get; set; } = new();
        public Exception? ConsolidatedException { get; set; }

        public Task AdicionarInvestimentoAsync(Investimento investimento) => Task.CompletedTask;
        public Task<CriarInvestimentoResponse> CriarInvestimentoAsync(CriarInvestimentoRequest request)
        {
            LastRequest = request;
            if (CreateException != null)
                throw CreateException;
            return Task.FromResult(CreateResponse);
        }
        public Task<InvestimentoResponse> AtualizarInvestimentoAsync(Guid id, AtualizarInvestimentoRequest request)
        {
            LastUpdateRequest = request;
            if (UpdateException != null)
                throw UpdateException;
            return Task.FromResult(UpdateResponse);
        }
        public Task<Investimento> GetInvestimentoByIdAsync(Guid id) => throw new NotImplementedException();
        public Task<InvestimentoResponse> ObterPorIdAsync(Guid id) => Task.FromResult(UpdateResponse);
        public Task<CarteiraInvestimentosConsolidadaResponse> ObterConsolidadoAsync()
        {
            if (ConsolidatedException != null) throw ConsolidatedException;
            return Task.FromResult(ConsolidatedResponse);
        }
        public Task RemoverInvestimentoAsync(Guid id) => Task.CompletedTask;
        public Task AtualizarAsync(Investimento investimento) => Task.CompletedTask;
        public Task<List<InvestimentoResponse>> ListarAsync() => Task.FromResult(new List<InvestimentoResponse>());
        public Task<List<InvestimentoResponse>> ObterPorMesAnoAsync(int mes, int ano) => Task.FromResult(new List<InvestimentoResponse>());
    }
}
