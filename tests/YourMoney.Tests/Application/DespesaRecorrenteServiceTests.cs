using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Application.Services;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;
using YourMoney.Domain.Repositories;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application
{
    public static class DespesaRecorrenteServiceTests
    {
        public static async Task ListSuggestionsCreatesPendingMonthlyOccurrenceWithoutExpense()
        {
            var repository = new InMemoryDespesaRecorrenteRepository();
            var despesaService = new FakeDespesaService();
            var service = CreateService(repository, despesaService);

            await service.CriarAsync(Request());
            var response = await service.ListarSugestoesAsync(5, 2026);
            await service.ListarSugestoesAsync(5, 2026);

            TestAssert.Equal(1, repository.Recorrencias.Count, "Create recurrence should persist base configuration");
            TestAssert.Equal(1, repository.Ocorrencias.Count, "Listing the same month twice should not duplicate pending occurrence");
            TestAssert.Equal(1, response.Itens.Count, "List suggestions should expose generated occurrence");
            TestAssert.Equal("Pendente", response.Itens[0].Status, "Generated occurrence should be pending");
            TestAssert.True(despesaService.LastCriarDespesaRequest == null, "Listing suggestions should not create a confirmed expense");
        }

        public static async Task ConfirmSuggestionCreatesExpenseAndFinalizesOccurrence()
        {
            var repository = new InMemoryDespesaRecorrenteRepository();
            var despesaId = Guid.NewGuid();
            var despesaService = new FakeDespesaService
            {
                CriarDespesaResponse = new CriarDespesaResponse { Id = despesaId },
                QueryResponse = new List<DespesaDTO>
                {
                    new DespesaDTO { Id = despesaId, Descricao = "Internet ajustada", Valor = 110m }
                }
            };
            var service = CreateService(repository, despesaService);

            await service.CriarAsync(Request());
            var sugestao = (await service.ListarSugestoesAsync(5, 2026)).Itens.Single();

            var response = await service.ConfirmarSugestaoAsync(sugestao.OcorrenciaId, new ConfirmarSugestaoDespesaRecorrenteRequest
            {
                Descricao = "Internet ajustada",
                Valor = 110m,
                Data = new DateTime(2026, 5, 12),
                IdContaFinanceira = DespesaTestFixtures.ContaLazerId,
                IdTipoDespesa = DespesaTestFixtures.TipoEssencialId,
                IdNaturezaDespesa = DespesaTestFixtures.NaturezaMoradiaId,
                IdCategoria = DespesaTestFixtures.NaturezaMoradiaId
            });

            var occurrence = repository.Ocorrencias.Single();
            TestAssert.Equal(StatusDespesaRecorrenteOcorrencia.Confirmada, occurrence.Status, "Confirming suggestion should finalize occurrence");
            TestAssert.Equal(despesaId, occurrence.DespesaConfirmadaId!.Value, "Occurrence should reference confirmed expense id");
            TestAssert.Equal(despesaId, response.Id, "Confirm response should return created expense DTO");
            TestAssert.True(despesaService.LastCriarDespesaRequest != null, "Confirming suggestion should create an expense");
            TestAssert.Equal("Internet ajustada", despesaService.LastCriarDespesaRequest!.Descricao, "Edited description should be used");
            TestAssert.Equal(110m, despesaService.LastCriarDespesaRequest.Valor, "Edited value should be used");
            TestAssert.Equal(new DateTime(2026, 5, 12), despesaService.LastCriarDespesaRequest.Data, "Edited date should be used");
            TestAssert.Equal(new DateTime(2026, 5, 1), despesaService.LastCriarDespesaRequest.MesReferencia, "Confirmed expense should keep suggestion month as reference");
        }

        public static async Task IgnoredSuggestionDoesNotBlockFollowingMonths()
        {
            var repository = new InMemoryDespesaRecorrenteRepository();
            var service = CreateService(repository, new FakeDespesaService());

            await service.CriarAsync(Request());
            var maio = (await service.ListarSugestoesAsync(5, 2026)).Itens.Single();

            await service.IgnorarSugestaoAsync(maio.OcorrenciaId);
            var junho = await service.ListarSugestoesAsync(6, 2026);

            TestAssert.Equal(2, repository.Ocorrencias.Count, "Ignoring one month should not block later generated suggestions");
            TestAssert.Equal(StatusDespesaRecorrenteOcorrencia.Ignorada, repository.Ocorrencias.Single(o => o.Id == maio.OcorrenciaId).Status, "Ignored occurrence should be finalized as ignored");
            TestAssert.Equal(1, junho.Itens.Count, "Following month should receive a new suggestion");
            TestAssert.Equal("Pendente", junho.Itens[0].Status, "Following month suggestion should be pending");
        }

        public static async Task RejectsInvalidCategoryHierarchy()
        {
            var service = CreateService(new InMemoryDespesaRecorrenteRepository(), new FakeDespesaService());
            var request = Request();
            request.IdNaturezaDespesa = DespesaTestFixtures.NaturezaMoradiaId;
            request.IdCategoria = DespesaTestFixtures.CategoriaCinemaId;

            await TestAssert.ThrowsAsync<ArgumentException>(
                () => service.CriarAsync(request),
                "Recurrence should reject category outside selected nature");
        }

        public static async Task UpdatesDeactivatesAndEndsRecurrenceWithoutChangingFinalizedOccurrences()
        {
            var repository = new InMemoryDespesaRecorrenteRepository();
            var service = CreateService(repository, new FakeDespesaService());
            var created = await service.CriarAsync(Request());
            var maio = (await service.ListarSugestoesAsync(5, 2026)).Itens.Single();

            await service.IgnorarSugestaoAsync(maio.OcorrenciaId);

            var update = Request();
            update.Descricao = "Internet fibra";
            update.ValorPrevisto = 130m;
            update.DataVencimento = new DateTime(2026, 6, 15);
            var updated = await service.AtualizarAsync(created.Id, update);
            var ended = await service.EncerrarAsync(created.Id, new EncerrarDespesaRecorrenteRequest
            {
                DataTermino = new DateTime(2026, 6, 30)
            });
            await service.DesativarAsync(created.Id);
            var julho = await service.ListarSugestoesAsync(7, 2026);

            TestAssert.Equal("Internet fibra", updated.Descricao, "Update should change base recurrence data");
            TestAssert.Equal(15, updated.DiaVencimento, "Update should change due day for future suggestions");
            TestAssert.Equal(new DateTime(2026, 6, 30), ended.DataTermino!.Value, "End should persist end date");
            TestAssert.True(!repository.Recorrencias.Single().Ativa, "Deactivate should mark recurrence inactive");
            TestAssert.Equal(StatusDespesaRecorrenteOcorrencia.Ignorada, repository.Ocorrencias.Single(o => o.Id == maio.OcorrenciaId).Status, "Update/end/deactivate should preserve finalized occurrence");
            TestAssert.Equal(0, julho.Itens.Count, "Inactive recurrence should not generate future suggestions");
        }

        private static DespesaRecorrenteService CreateService(
            InMemoryDespesaRecorrenteRepository repository,
            FakeDespesaService despesaService)
        {
            return new DespesaRecorrenteService(
                repository,
                despesaService,
                new ContaFinanceiraRepositoryStub(true),
                new CategoriaRepositoryStub(true),
                new FakeCurrentUserService());
        }

        private static DespesaRecorrenteRequest Request()
        {
            return new DespesaRecorrenteRequest
            {
                Descricao = "Internet",
                ValorPrevisto = 100m,
                IdContaFinanceira = DespesaTestFixtures.ContaId,
                IdTipoDespesa = DespesaTestFixtures.TipoEssencialId,
                IdNaturezaDespesa = DespesaTestFixtures.NaturezaMoradiaId,
                IdCategoria = DespesaTestFixtures.CategoriaAluguelId,
                DataVencimento = new DateTime(2026, 5, 10),
                DataInicio = new DateTime(2026, 5, 1)
            };
        }
    }

    public class InMemoryDespesaRecorrenteRepository : IDespesaRecorrenteRepository
    {
        public List<DespesaRecorrente> Recorrencias { get; } = new();
        public List<DespesaRecorrenteOcorrencia> Ocorrencias { get; } = new();

        public Task AdicionarAsync(DespesaRecorrente recorrencia)
        {
            Recorrencias.Add(recorrencia);
            return Task.CompletedTask;
        }

        public Task AtualizarAsync(DespesaRecorrente recorrencia) => Task.CompletedTask;

        public Task<DespesaRecorrente?> ObterPorIdAsync(Guid id, string usuarioId)
        {
            return Task.FromResult(Recorrencias.FirstOrDefault(r => r.Id == id && r.UsuarioId == usuarioId));
        }

        public Task<List<DespesaRecorrente>> ListarAsync(string usuarioId, bool? ativas = null)
        {
            return Task.FromResult(Recorrencias
                .Where(r => r.UsuarioId == usuarioId)
                .Where(r => ativas == null || r.Ativa == ativas.Value)
                .ToList());
        }

        public Task<List<DespesaRecorrente>> ListarElegiveisParaMesAsync(string usuarioId, DateTime mesReferencia)
        {
            return Task.FromResult(Recorrencias
                .Where(r => r.UsuarioId == usuarioId && r.EstaElegivelParaMes(mesReferencia))
                .ToList());
        }

        public Task<DespesaRecorrenteOcorrencia?> ObterOcorrenciaAsync(Guid despesaRecorrenteId, DateTime mesReferencia, string usuarioId)
        {
            var mes = DespesaRecorrente.NormalizarMesReferencia(mesReferencia);
            var ocorrencia = Ocorrencias.FirstOrDefault(o =>
                o.DespesaRecorrenteId == despesaRecorrenteId &&
                o.MesReferencia == mes &&
                o.UsuarioId == usuarioId);

            VincularRecorrencia(ocorrencia);
            return Task.FromResult(ocorrencia);
        }

        public Task<DespesaRecorrenteOcorrencia?> ObterOcorrenciaPorIdAsync(Guid ocorrenciaId, string usuarioId)
        {
            var ocorrencia = Ocorrencias.FirstOrDefault(o => o.Id == ocorrenciaId && o.UsuarioId == usuarioId);
            VincularRecorrencia(ocorrencia);
            return Task.FromResult(ocorrencia);
        }

        public Task<List<DespesaRecorrenteOcorrencia>> ListarOcorrenciasPorMesAsync(string usuarioId, DateTime mesReferencia)
        {
            var mes = DespesaRecorrente.NormalizarMesReferencia(mesReferencia);
            var ocorrencias = Ocorrencias
                .Where(o => o.UsuarioId == usuarioId && o.MesReferencia == mes)
                .ToList();

            ocorrencias.ForEach(VincularRecorrencia);
            return Task.FromResult(ocorrencias);
        }

        public Task AdicionarOcorrenciaAsync(DespesaRecorrenteOcorrencia ocorrencia)
        {
            VincularRecorrencia(ocorrencia);
            Ocorrencias.Add(ocorrencia);
            return Task.CompletedTask;
        }

        public Task AtualizarOcorrenciaAsync(DespesaRecorrenteOcorrencia ocorrencia) => Task.CompletedTask;

        private void VincularRecorrencia(DespesaRecorrenteOcorrencia? ocorrencia)
        {
            if (ocorrencia == null)
                return;

            var recorrencia = Recorrencias.FirstOrDefault(r => r.Id == ocorrencia.DespesaRecorrenteId);
            if (recorrencia != null)
            {
                typeof(DespesaRecorrenteOcorrencia)
                    .GetProperty(nameof(DespesaRecorrenteOcorrencia.DespesaRecorrente))!
                    .SetValue(ocorrencia, recorrencia);
            }
        }
    }
}
