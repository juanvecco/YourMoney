using YourMoney.Application.DTOs;
using YourMoney.Application.Services;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;
using YourMoney.Domain.Repositories;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application
{
    public static class ReceitaRecorrenteServiceTests
    {
        public static async Task CreatesListsAndDoesNotAutomaticallyCreateRevenue()
        {
            var repository = new InMemoryReceitaRecorrenteRepository();
            var receitaService = new FakeReceitaService();
            var service = CriarService(repository, receitaService);

            var created = await service.CriarAsync(Request());
            var listed = await service.ListarAsync();

            TestAssert.Equal(1, repository.Recorrencias.Count, "Create should persist one recurrence");
            TestAssert.Equal("test-user", repository.Recorrencias[0].UsuarioId, "Recurrence should use authenticated owner");
            TestAssert.Equal(created.Id, listed.Itens.Single().Id, "List should return the persisted recurrence");
            TestAssert.True(receitaService.LastRequest == null, "Creating recurrence should not automatically create Receita");
        }

        public static async Task MaterializesAndFinalizesMonthlySuggestions()
        {
            var repository = new InMemoryReceitaRecorrenteRepository();
            var receitaService = new FakeReceitaService
            {
                CreateResponse = new CriarReceitaResponse
                {
                    Id = Guid.NewGuid(),
                    Descricao = "Salário",
                    Valor = 5100m,
                    Data = new DateTime(2026, 7, 6),
                    MesReferencia = new DateTime(2026, 7, 1),
                    Natureza = NaturezaReceita.RendaDisponivel.ToString()
                }
            };
            var service = CriarService(repository, receitaService);
            await service.CriarAsync(Request());

            var suggestions = await service.ListarSugestoesAsync(7, 2026);
            var suggestion = suggestions.Itens.Single();
            var confirmed = await service.ConfirmarSugestaoAsync(suggestion.OcorrenciaId, new ConfirmarSugestaoReceitaRecorrenteRequest
            {
                Valor = 5100m,
                Data = new DateTime(2026, 7, 6)
            });

            TestAssert.Equal(1, repository.Ocorrencias.Count, "Month should materialize one occurrence");
            TestAssert.Equal(StatusReceitaRecorrenteOcorrencia.Confirmada, repository.Ocorrencias[0].Status, "Confirmation should finalize occurrence");
            TestAssert.Equal(repository.Recorrencias[0].IdContaFinanceira, receitaService.LastRequest!.IdContaFinanceira!.Value, "Confirmation should carry recurrence account");
            TestAssert.Equal(new DateTime(2026, 7, 1), receitaService.LastRequest.MesReferencia, "Confirmation should use suggestion month as reference");
            TestAssert.Equal(confirmed.Id, repository.Ocorrencias[0].ReceitaConfirmadaId!.Value, "Occurrence should link created Receita");

            await TestAssert.ThrowsAsync<InvalidOperationException>(
                () => service.IgnorarSugestaoAsync(suggestion.OcorrenciaId),
                "Finalized suggestion should reject a second finalization");
        }

        public static async Task IgnoresOnlySelectedMonth()
        {
            var repository = new InMemoryReceitaRecorrenteRepository();
            var service = CriarService(repository, new FakeReceitaService());
            await service.CriarAsync(Request());
            var july = (await service.ListarSugestoesAsync(7, 2026)).Itens.Single();

            await service.IgnorarSugestaoAsync(july.OcorrenciaId);
            var august = await service.ListarSugestoesAsync(8, 2026);

            TestAssert.Equal(StatusReceitaRecorrenteOcorrencia.Ignorada, repository.Ocorrencias.Single(o => o.Id == july.OcorrenciaId).Status, "Selected month should be ignored");
            TestAssert.Equal(1, august.Itens.Count, "Following month should receive a new pending suggestion");
        }

        public static async Task CalculatesSeparateExactReserveProjections()
        {
            var repository = new InMemoryReceitaRecorrenteRepository();
            var service = CriarService(repository, new FakeReceitaService());
            await service.CriarAsync(Request(valor: 5000.01m));
            await service.CriarAsync(Request(valor: 3500.02m));

            var projection = await service.ObterProjecaoReservaAsync();

            TestAssert.Equal(2, projection.Itens.Count, "Equal descriptions should remain separate projection rows");
            TestAssert.Equal(30000.06m, projection.Itens[0].ValorSeisMeses, "Six-month projection should use exact decimal multiplication");
            TestAssert.Equal(42000.24m, projection.Itens[1].ValorDozeMeses, "Twelve-month projection should use exact decimal multiplication");
        }

        public static async Task RejectsForeignAccountAndPersistsIndependentFlags()
        {
            var foreignService = new ReceitaRecorrenteService(
                new InMemoryReceitaRecorrenteRepository(),
                new FakeReceitaService(),
                new ContaFinanceiraRepositoryStub(false),
                new FakeCurrentUserService());
            await TestAssert.ThrowsAsync<ArgumentException>(
                () => foreignService.CriarAsync(Request()),
                "Foreign account should be rejected");

            var repository = new InMemoryReceitaRecorrenteRepository();
            var service = CriarService(repository, new FakeReceitaService());
            var request = Request();
            request.EhSalario = true;
            request.ConsideraReservaEmergencia = false;
            var created = await service.CriarAsync(request);

            TestAssert.True(created.EhSalario, "Salary flag should persist as true");
            TestAssert.True(!created.ConsideraReservaEmergencia, "Reserve flag should persist independently as false");
        }

        public static async Task MaintenanceStopsFutureViewsAndPreservesFinalizedOccurrence()
        {
            var repository = new InMemoryReceitaRecorrenteRepository();
            var service = CriarService(repository, new FakeReceitaService());
            var created = await service.CriarAsync(Request());
            var july = (await service.ListarSugestoesAsync(7, 2026)).Itens.Single();
            await service.IgnorarSugestaoAsync(july.OcorrenciaId);

            var update = Request(valor: 5200m);
            update.Descricao = "Salário atualizado";
            await service.AtualizarAsync(created.Id, update);
            await service.EncerrarAsync(created.Id, new EncerrarReceitaRecorrenteRequest { DataTermino = new DateTime(2026, 8, 31) });
            await service.DesativarAsync(created.Id);
            var september = await service.ListarSugestoesAsync(9, 2026);

            TestAssert.Equal("Salário atualizado", repository.Recorrencias.Single().Descricao, "Update should change recurrence data");
            TestAssert.Equal(StatusReceitaRecorrenteOcorrencia.Ignorada, repository.Ocorrencias.Single().Status, "Maintenance should preserve finalized history");
            TestAssert.Equal(0, september.Itens.Count, "Inactive recurrence should stop future suggestions");
        }

        private static ReceitaRecorrenteService CriarService(InMemoryReceitaRecorrenteRepository repository, FakeReceitaService receitaService)
        {
            return new ReceitaRecorrenteService(
                repository,
                receitaService,
                new ContaFinanceiraRepositoryStub(true),
                new FakeCurrentUserService());
        }

        private static ReceitaRecorrenteRequest Request(decimal valor = 5000m)
        {
            return new ReceitaRecorrenteRequest
            {
                Descricao = "Salário",
                ValorPrevisto = valor,
                IdContaFinanceira = DespesaTestFixtures.ContaId,
                Natureza = NaturezaReceita.RendaDisponivel.ToString(),
                EhSalario = true,
                ConsideraReservaEmergencia = true,
                DataRecebimento = new DateTime(2026, 1, 5),
                DataInicio = new DateTime(2026, 1, 1)
            };
        }
    }

    public class InMemoryReceitaRecorrenteRepository : IReceitaRecorrenteRepository
    {
        public List<ReceitaRecorrente> Recorrencias { get; } = new();
        public List<ReceitaRecorrenteOcorrencia> Ocorrencias { get; } = new();

        public Task AdicionarAsync(ReceitaRecorrente recorrencia)
        {
            VincularConta(recorrencia);
            Recorrencias.Add(recorrencia);
            return Task.CompletedTask;
        }

        public Task AtualizarAsync(ReceitaRecorrente recorrencia) => Task.CompletedTask;
        public Task<ReceitaRecorrente?> ObterPorIdAsync(Guid id, string usuarioId) =>
            Task.FromResult(Recorrencias.FirstOrDefault(r => r.Id == id && r.UsuarioId == usuarioId));
        public Task<List<ReceitaRecorrente>> ListarAsync(string usuarioId, bool? ativas = null) =>
            Task.FromResult(Recorrencias.Where(r => r.UsuarioId == usuarioId && (!ativas.HasValue || r.Ativa == ativas)).ToList());
        public Task<List<ReceitaRecorrente>> ListarElegiveisParaMesAsync(string usuarioId, DateTime mesReferencia) =>
            Task.FromResult(Recorrencias.Where(r => r.UsuarioId == usuarioId && r.EstaElegivelParaMes(mesReferencia)).ToList());
        public Task<List<ReceitaRecorrente>> ListarElegiveisParaReservaAsync(string usuarioId, DateTime mesReferencia) =>
            Task.FromResult(Recorrencias.Where(r => r.UsuarioId == usuarioId && r.EstaElegivelParaReserva(mesReferencia)).ToList());
        public Task<List<ReceitaRecorrente>> ListarElegiveisParaInvestimentoAsync(string usuarioId, DateTime mesReferencia) =>
            Task.FromResult(Recorrencias.Where(r => r.UsuarioId == usuarioId && r.EhSalario && r.Natureza == YourMoney.Domain.Enums.NaturezaReceita.RendaDisponivel && r.EstaElegivelParaMes(mesReferencia)).ToList());
        public Task<List<ReceitaRecorrente>> ListarReservasSalariaisAtivasAsync(string usuarioId, DateTime mesReferencia) =>
            Task.FromResult(Recorrencias.Where(r => r.UsuarioId == usuarioId && r.EhSalario && r.Natureza == YourMoney.Domain.Enums.NaturezaReceita.RendaDisponivel && r.EstaElegivelParaReserva(mesReferencia)).ToList());

        public Task AdicionarOcorrenciaAsync(ReceitaRecorrenteOcorrencia ocorrencia)
        {
            VincularRecorrencia(ocorrencia);
            Ocorrencias.Add(ocorrencia);
            return Task.CompletedTask;
        }

        public Task AtualizarOcorrenciaAsync(ReceitaRecorrenteOcorrencia ocorrencia) => Task.CompletedTask;

        public Task<ReceitaRecorrenteOcorrencia?> ObterOcorrenciaAsync(Guid recorrenciaId, DateTime mesReferencia, string usuarioId)
        {
            var mes = ReceitaRecorrente.NormalizarMesReferencia(mesReferencia);
            return Task.FromResult(Ocorrencias.FirstOrDefault(o => o.UsuarioId == usuarioId && o.ReceitaRecorrenteId == recorrenciaId && o.MesReferencia == mes));
        }

        public Task<ReceitaRecorrenteOcorrencia?> ObterOcorrenciaPorIdAsync(Guid ocorrenciaId, string usuarioId)
        {
            var ocorrencia = Ocorrencias.FirstOrDefault(o => o.Id == ocorrenciaId && o.UsuarioId == usuarioId);
            VincularRecorrencia(ocorrencia);
            return Task.FromResult(ocorrencia);
        }

        public Task<List<ReceitaRecorrenteOcorrencia>> ListarOcorrenciasPorMesAsync(string usuarioId, DateTime mesReferencia)
        {
            var mes = ReceitaRecorrente.NormalizarMesReferencia(mesReferencia);
            var itens = Ocorrencias.Where(o => o.UsuarioId == usuarioId && o.MesReferencia == mes).ToList();
            itens.ForEach(VincularRecorrencia);
            return Task.FromResult(itens);
        }

        private void VincularRecorrencia(ReceitaRecorrenteOcorrencia? ocorrencia)
        {
            if (ocorrencia == null) return;
            var recorrencia = Recorrencias.FirstOrDefault(r => r.Id == ocorrencia.ReceitaRecorrenteId);
            if (recorrencia != null)
                typeof(ReceitaRecorrenteOcorrencia).GetProperty(nameof(ReceitaRecorrenteOcorrencia.ReceitaRecorrente))!.SetValue(ocorrencia, recorrencia);
        }

        private static void VincularConta(ReceitaRecorrente recorrencia)
        {
            var conta = new ContaFinanceira("Conta Principal", recorrencia.UsuarioId);
            typeof(ContaFinanceira).GetProperty(nameof(ContaFinanceira.Id))!.SetValue(conta, recorrencia.IdContaFinanceira);
            typeof(ReceitaRecorrente).GetProperty(nameof(ReceitaRecorrente.ContaFinanceira))!.SetValue(recorrencia, conta);
        }
    }
}
