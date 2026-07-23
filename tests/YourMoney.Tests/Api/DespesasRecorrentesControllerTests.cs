using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Api.Controllers;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api
{
    public static class DespesasRecorrentesControllerTests
    {
        public static async Task PostRecurrenceReturnsCreatedContract()
        {
            var response = Response();
            var service = new FakeDespesaRecorrenteService { CriarResponse = response };
            var controller = new DespesasRecorrentesController(service);

            var result = await controller.Criar(Request());

            var created = result as CreatedAtActionResult;
            TestAssert.True(created != null, "POST recurrence should return CreatedAtActionResult");
            TestAssert.Equal(nameof(DespesasRecorrentesController.ObterPorId), created!.ActionName, "POST recurrence should link to get by id");
            TestAssert.Equal(response, created.Value, "POST recurrence should return typed response");
            TestAssert.True(service.LastCriarRequest != null, "POST recurrence should pass typed request to service");
        }

        public static async Task ConfirmSuggestionReturnsCreatedExpenseContract()
        {
            var despesaId = Guid.NewGuid();
            var service = new FakeDespesaRecorrenteService
            {
                ConfirmarResponse = new DespesaDTO
                {
                    Id = despesaId,
                    Descricao = "Internet",
                    Valor = 100m,
                    Data = new DateTime(2026, 5, 10),
                    IdContaFinanceira = DespesaTestFixtures.ContaId,
                    IdCategoria = DespesaTestFixtures.CategoriaAluguelId
                }
            };
            var controller = new DespesasRecorrentesController(service);
            var ocorrenciaId = Guid.NewGuid();

            var result = await controller.ConfirmarSugestao(ocorrenciaId, new ConfirmarSugestaoDespesaRecorrenteRequest());

            var created = result as CreatedAtActionResult;
            TestAssert.True(created != null, "Confirm suggestion should return CreatedAtActionResult");
            TestAssert.Equal(nameof(DespesasController.GetDespesaById), created!.ActionName, "Confirm suggestion should link to expense get by id");
            TestAssert.Equal("Despesas", created.ControllerName, "Confirm suggestion should target Despesas controller");
            TestAssert.Equal(service.ConfirmarResponse, created.Value, "Confirm suggestion should return created expense DTO");
            TestAssert.Equal(ocorrenciaId, service.LastConfirmarOcorrenciaId, "Confirm suggestion should pass occurrence id");
        }

        public static async Task FinalizedSuggestionReturnsConflict()
        {
            var service = new FakeDespesaRecorrenteService
            {
                ConfirmarException = new InvalidOperationException("Sugestão mensal já finalizada.")
            };

            var result = await new DespesasRecorrentesController(service)
                .ConfirmarSugestao(Guid.NewGuid(), new ConfirmarSugestaoDespesaRecorrenteRequest());

            TestAssert.True(result is ConflictObjectResult, "Finalized suggestion should return conflict");
        }

        public static async Task PutAndPatchMaintenanceReturnExpectedContracts()
        {
            var response = Response();
            var service = new FakeDespesaRecorrenteService { DefaultResponse = response };
            var controller = new DespesasRecorrentesController(service);
            var id = Guid.NewGuid();

            var put = await controller.Atualizar(id, Request());
            var encerrar = await controller.Encerrar(id, new EncerrarDespesaRecorrenteRequest { DataTermino = new DateTime(2026, 6, 30) });
            var desativar = await controller.Desativar(id);

            TestAssert.True(put is OkObjectResult, "PUT recurrence should return OK");
            TestAssert.True(encerrar is OkObjectResult, "PATCH encerrar should return OK");
            TestAssert.True(desativar is NoContentResult, "PATCH desativar should return NoContent");
            TestAssert.Equal(id, service.LastAtualizarId, "PUT recurrence should pass route id");
            TestAssert.Equal(id, service.LastEncerrarId, "PATCH encerrar should pass route id");
            TestAssert.Equal(id, service.LastDesativarId, "PATCH desativar should pass route id");
        }

        public static Task RecurrenceControllerRequiresAuthorization()
        {
            var controllerHasAuthorize = typeof(DespesasRecorrentesController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Any();

            TestAssert.True(controllerHasAuthorize, "Recurring expenses controller should require authorization");
            return Task.CompletedTask;
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

        private static DespesaRecorrenteResponse Response()
        {
            return new DespesaRecorrenteResponse
            {
                Id = Guid.NewGuid(),
                Descricao = "Internet",
                ValorPrevisto = 100m,
                IdContaFinanceira = DespesaTestFixtures.ContaId,
                ContaDescricao = "Conta Principal",
                IdTipoDespesa = DespesaTestFixtures.TipoEssencialId,
                TipoDescricao = "Essencial",
                IdNaturezaDespesa = DespesaTestFixtures.NaturezaMoradiaId,
                NaturezaDescricao = "Moradia",
                IdCategoria = DespesaTestFixtures.CategoriaAluguelId,
                CategoriaDescricao = "Aluguel",
                DiaVencimento = 10,
                DataInicio = new DateTime(2026, 5, 1),
                Ativa = true
            };
        }
    }

    public class FakeDespesaRecorrenteService : IDespesaRecorrenteService
    {
        public DespesaRecorrenteResponse CriarResponse { get; set; } = new();
        public DespesaRecorrenteResponse DefaultResponse { get; set; } = new();
        public DespesaDTO ConfirmarResponse { get; set; } = new();
        public Exception? ConfirmarException { get; set; }
        public DespesaRecorrenteRequest? LastCriarRequest { get; private set; }
        public Guid LastConfirmarOcorrenciaId { get; private set; }
        public Guid LastAtualizarId { get; private set; }
        public Guid LastDesativarId { get; private set; }
        public Guid LastEncerrarId { get; private set; }

        public Task<DespesaRecorrenteResponse> CriarAsync(DespesaRecorrenteRequest request)
        {
            LastCriarRequest = request;
            return Task.FromResult(CriarResponse);
        }

        public Task<ListarDespesasRecorrentesResponse> ListarAsync(bool? ativas = null)
        {
            return Task.FromResult(new ListarDespesasRecorrentesResponse());
        }

        public Task<DespesaRecorrenteResponse> ObterPorIdAsync(Guid id)
        {
            return Task.FromResult(DefaultResponse);
        }

        public Task<DespesaRecorrenteResponse> AtualizarAsync(Guid id, DespesaRecorrenteRequest request)
        {
            LastAtualizarId = id;
            return Task.FromResult(DefaultResponse);
        }

        public Task DesativarAsync(Guid id)
        {
            LastDesativarId = id;
            return Task.CompletedTask;
        }

        public Task<DespesaRecorrenteResponse> EncerrarAsync(Guid id, EncerrarDespesaRecorrenteRequest request)
        {
            LastEncerrarId = id;
            return Task.FromResult(DefaultResponse);
        }

        public Task<ListarSugestoesDespesasRecorrentesResponse> ListarSugestoesAsync(int mes, int ano)
        {
            return Task.FromResult(new ListarSugestoesDespesasRecorrentesResponse { Mes = mes, Ano = ano });
        }

        public Task<DespesaDTO> ConfirmarSugestaoAsync(Guid ocorrenciaId, ConfirmarSugestaoDespesaRecorrenteRequest request)
        {
            LastConfirmarOcorrenciaId = ocorrenciaId;
            if (ConfirmarException != null)
                throw ConfirmarException;

            return Task.FromResult(ConfirmarResponse);
        }

        public Task IgnorarSugestaoAsync(Guid ocorrenciaId) => Task.CompletedTask;
    }
}
