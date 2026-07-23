using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Api.Controllers;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api
{
    public static class ReceitasRecorrentesControllerTests
    {
        public static async Task CrudReturnsExpectedContracts()
        {
            var response = Response();
            var service = new FakeReceitaRecorrenteService { DefaultResponse = response };
            var controller = new ReceitasRecorrentesController(service);
            var id = response.Id;

            var post = await controller.Criar(Request());
            var get = await controller.ObterPorId(id);
            var put = await controller.Atualizar(id, Request());
            var end = await controller.Encerrar(id, new EncerrarReceitaRecorrenteRequest { DataTermino = new DateTime(2026, 12, 31) });
            var deactivate = await controller.Desativar(id);

            TestAssert.True(post is CreatedAtActionResult, "POST should return created contract");
            TestAssert.True(get is OkObjectResult, "GET by id should return OK");
            TestAssert.True(put is OkObjectResult, "PUT should return OK");
            TestAssert.True(end is OkObjectResult, "End should return updated recurrence");
            TestAssert.True(deactivate is NoContentResult, "Deactivate should return no content");
        }

        public static async Task SuggestionsReturnCreatedNoContentAndConflictContracts()
        {
            var service = new FakeReceitaRecorrenteService();
            var controller = new ReceitasRecorrentesController(service);
            var id = Guid.NewGuid();

            var list = await controller.ListarSugestoes(7, 2026);
            var confirm = await controller.Confirmar(id, new ConfirmarSugestaoReceitaRecorrenteRequest());
            var ignore = await controller.Ignorar(id);
            service.Finalized = true;
            var conflict = await controller.Confirmar(id, new ConfirmarSugestaoReceitaRecorrenteRequest());

            TestAssert.True(list is OkObjectResult, "Suggestion list should return OK");
            TestAssert.Equal(201, ((ObjectResult)confirm).StatusCode!.Value, "Confirmation should return 201");
            TestAssert.True(ignore is NoContentResult, "Ignore should return no content");
            TestAssert.True(conflict is ConflictObjectResult, "Second finalization should return conflict");
        }

        public static async Task ProjectionReturnsItemsAndEmptyContract()
        {
            var service = new FakeReceitaRecorrenteService
            {
                Projection = new ProjecaoReservaEmergenciaResponse
                {
                    Itens = new List<ProjecaoReservaEmergenciaItemResponse>
                    {
                        new() { ReceitaRecorrenteId = Guid.NewGuid(), Descricao = "Salário", ValorMensal = 5000m, ValorSeisMeses = 30000m, ValorDozeMeses = 60000m }
                    }
                }
            };
            var controller = new ReceitasRecorrentesController(service);

            var populated = (OkObjectResult)await controller.ObterReservaEmergencia();
            service.Projection = new ProjecaoReservaEmergenciaResponse();
            var empty = (OkObjectResult)await controller.ObterReservaEmergencia();

            TestAssert.Equal(1, ((ProjecaoReservaEmergenciaResponse)populated.Value!).Itens.Count, "Projection should preserve individual rows");
            TestAssert.Equal(0, ((ProjecaoReservaEmergenciaResponse)empty.Value!).Itens.Count, "Empty projection should return an empty items array");
        }

        public static async Task MapsValidationNotFoundAndAuthorization()
        {
            var service = new FakeReceitaRecorrenteService { ValidationFailure = true };
            var controller = new ReceitasRecorrentesController(service);
            var validation = await controller.Criar(Request());
            service.ValidationFailure = false;
            service.NotFound = true;
            var notFound = await controller.ObterPorId(Guid.NewGuid());
            var authorized = typeof(ReceitasRecorrentesController).GetCustomAttributes(typeof(AuthorizeAttribute), true).Any();

            TestAssert.True(validation is BadRequestObjectResult, "Validation failures should return 400");
            TestAssert.True(notFound is NotFoundObjectResult, "Unknown user-owned resource should return 404");
            TestAssert.True(authorized, "Recurring revenue controller should require authorization");
        }

        private static ReceitaRecorrenteRequest Request() => new()
        {
            Descricao = "Salário",
            ValorPrevisto = 5000m,
            IdContaFinanceira = DespesaTestFixtures.ContaId,
            Natureza = "RendaDisponivel",
            EhSalario = true,
            ConsideraReservaEmergencia = true,
            DataRecebimento = new DateTime(2026, 7, 5),
            DataInicio = new DateTime(2026, 1, 1)
        };

        private static ReceitaRecorrenteResponse Response() => new()
        {
            Id = Guid.NewGuid(),
            Descricao = "Salário",
            ValorPrevisto = 5000m,
            IdContaFinanceira = DespesaTestFixtures.ContaId,
            ContaDescricao = "Principal",
            Natureza = "RendaDisponivel",
            EhSalario = true,
            ConsideraReservaEmergencia = true,
            DiaRecebimento = 5,
            DataInicio = new DateTime(2026, 1, 1),
            Ativa = true
        };
    }

    public class FakeReceitaRecorrenteService : IReceitaRecorrenteService
    {
        public ReceitaRecorrenteResponse DefaultResponse { get; set; } = ResponsePadrao();
        public ProjecaoReservaEmergenciaResponse Projection { get; set; } = new();
        public bool Finalized { get; set; }
        public bool ValidationFailure { get; set; }
        public bool NotFound { get; set; }

        public Task<ReceitaRecorrenteResponse> CriarAsync(ReceitaRecorrenteRequest request)
        {
            if (ValidationFailure) throw new ArgumentException("Dados inválidos.");
            return Task.FromResult(DefaultResponse);
        }
        public Task<ListarReceitasRecorrentesResponse> ListarAsync(bool? ativas = null) => Task.FromResult(new ListarReceitasRecorrentesResponse());
        public Task<ReceitaRecorrenteResponse> ObterPorIdAsync(Guid id)
        {
            if (NotFound) throw new InvalidOperationException("Receita recorrente não encontrada.");
            return Task.FromResult(DefaultResponse);
        }
        public Task<ReceitaRecorrenteResponse> AtualizarAsync(Guid id, ReceitaRecorrenteRequest request) => Task.FromResult(DefaultResponse);
        public Task DesativarAsync(Guid id) => Task.CompletedTask;
        public Task<ReceitaRecorrenteResponse> EncerrarAsync(Guid id, EncerrarReceitaRecorrenteRequest request) => Task.FromResult(DefaultResponse);
        public Task<ListarSugestoesReceitasRecorrentesResponse> ListarSugestoesAsync(int mes, int ano) =>
            Task.FromResult(new ListarSugestoesReceitasRecorrentesResponse { Mes = mes, Ano = ano });
        public Task<CriarReceitaResponse> ConfirmarSugestaoAsync(Guid ocorrenciaId, ConfirmarSugestaoReceitaRecorrenteRequest request)
        {
            if (Finalized) throw new InvalidOperationException("Sugestão mensal já finalizada.");
            return Task.FromResult(new CriarReceitaResponse { Id = Guid.NewGuid(), MesReferencia = new DateTime(2026, 7, 1) });
        }
        public Task IgnorarSugestaoAsync(Guid ocorrenciaId)
        {
            if (Finalized) throw new InvalidOperationException("Sugestão mensal já finalizada.");
            return Task.CompletedTask;
        }
        public Task<ProjecaoReservaEmergenciaResponse> ObterProjecaoReservaAsync() => Task.FromResult(Projection);

        private static ReceitaRecorrenteResponse ResponsePadrao() => new()
        {
            Id = Guid.NewGuid(),
            Descricao = "Salário",
            ValorPrevisto = 5000m,
            IdContaFinanceira = DespesaTestFixtures.ContaId,
            Natureza = "RendaDisponivel",
            DiaRecebimento = 5,
            DataInicio = new DateTime(2026, 1, 1),
            Ativa = true
        };
    }
}
