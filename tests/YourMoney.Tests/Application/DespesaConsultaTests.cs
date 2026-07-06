using YourMoney.Application.DTOs;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application
{
    public static class DespesaConsultaTests
    {
        public static async Task FiltersByAccountTypeNatureAndCombinedFilters()
        {
            var despesaRepository = CriarRepositorioComDespesas();
            var service = CriarServico(despesaRepository);

            var porConta = await service.ConsultarDespesasAsync(Request(idContaFinanceira: DespesaTestFixtures.ContaLazerId));
            TestAssert.Equal(1, porConta.TotalResultados, "Account filter should return only matching expenses");
            TestAssert.Equal("Cinema", porConta.Itens.Single().Descricao, "Account filter should preserve matching expense");

            var porTipo = await service.ConsultarDespesasAsync(Request(idTipoDespesa: DespesaTestFixtures.TipoEssencialId));
            TestAssert.Equal(3, porTipo.TotalResultados, "Type filter should include descendant categories");

            var porNatureza = await service.ConsultarDespesasAsync(Request(idNaturezaDespesa: DespesaTestFixtures.NaturezaMoradiaId));
            TestAssert.Equal(1, porNatureza.TotalResultados, "Nature filter should include specific categories below it");
            TestAssert.Equal("Aluguel", porNatureza.Itens.Single().Descricao, "Nature filter should include descendant expense");

            var combinado = await service.ConsultarDespesasAsync(Request(
                idContaFinanceira: DespesaTestFixtures.ContaId,
                idTipoDespesa: DespesaTestFixtures.TipoEssencialId,
                idNaturezaDespesa: DespesaTestFixtures.NaturezaMercadoId));

            TestAssert.Equal(1, combinado.TotalResultados, "Combined filters should use AND semantics");
            TestAssert.Equal("Mercado", combinado.Itens.Single().Descricao, "Combined filters should return the matching expense");

            var categoriaForaDoTipo = await service.ConsultarDespesasAsync(Request(
                idTipoDespesa: DespesaTestFixtures.TipoEssencialId,
                idNaturezaDespesa: DespesaTestFixtures.NaturezaPasseioId));

            TestAssert.Equal(0, categoriaForaDoTipo.TotalResultados, "Combined category filters from different branches should return no expenses");
        }

        public static async Task FilteredTotalUsesAllResultsBeforePagination()
        {
            var despesaRepository = CriarRepositorioComDespesas();
            var service = CriarServico(despesaRepository);

            var response = await service.ConsultarDespesasAsync(Request(idTipoDespesa: DespesaTestFixtures.TipoEssencialId, tamanhoPagina: 1));

            TestAssert.Equal(3, response.TotalResultados, "Response should count all filtered results");
            TestAssert.Equal(3, response.TotalPaginas, "Response should calculate total pages");
            TestAssert.Equal(1, response.Itens.Count, "Response should return only requested page items");
            TestAssert.Equal(1150m, response.ValorTotalFiltrado, "Totalizer should sum all filtered results before pagination");
        }

        public static async Task AccountDistributionUsesAllResultsBeforePagination()
        {
            var despesaRepository = CriarRepositorioComDespesas();
            var service = CriarServico(despesaRepository);

            var response = await service.ConsultarDespesasAsync(Request(tamanhoPagina: 1));

            TestAssert.Equal(1, response.Itens.Count, "Response should return only one page item");
            TestAssert.Equal(4, response.TotalResultados, "Response should count every filtered expense");
            TestAssert.Equal(2, response.TotaisPorConta.Count, "Distribution should include every filtered account");
            TestAssert.Equal(1150m, response.TotaisPorConta.Single(t => t.IdContaFinanceira == DespesaTestFixtures.ContaId).Valor, "Main account total should include all pages");
            TestAssert.Equal(80m, response.TotaisPorConta.Single(t => t.IdContaFinanceira == DespesaTestFixtures.ContaLazerId).Valor, "Second account total should include filtered expenses outside the current page");
        }

        public static async Task ReimbursedExpensesContributeLiquidValueToFilteredTotal()
        {
            var despesaRepository = new InMemoryDespesaRepository();
            var receitaRepository = new InMemoryReceitaRepository();
            var despesa = new Despesa("Compra para terceiro", 150m, new DateTime(2026, 7, 5), DespesaTestFixtures.ContaId, DespesaTestFixtures.CategoriaId, "test-user");
            despesaRepository.Despesas.Add(despesa);
            receitaRepository.Receitas.Add(new Receita(
                "Reembolso",
                50m,
                new DateTime(2026, 7, 6),
                new DateTime(2026, 7, 1),
                "test-user",
                NaturezaReceita.Reembolso,
                despesa.Id));

            var service = CriarServico(despesaRepository, receitaRepository);

            var response = await service.ConsultarDespesasAsync(Request());

            TestAssert.Equal(100m, response.ValorTotalFiltrado, "Totalizer should subtract reimbursements");
            TestAssert.Equal(100m, response.Itens.Single().ValorLiquido, "Item should expose the same liquid value used by totalizer");
        }

        public static async Task PaginationReturnsMetadataAndClampsOutOfRangePage()
        {
            var despesaRepository = CriarRepositorioComDespesas();
            var service = CriarServico(despesaRepository);

            var response = await service.ConsultarDespesasAsync(Request(pagina: 99, tamanhoPagina: 2));

            TestAssert.Equal(4, response.TotalResultados, "Pagination should count all filtered expenses");
            TestAssert.Equal(2, response.TotalPaginas, "Pagination should calculate total pages");
            TestAssert.Equal(2, response.PaginaAtual, "Pagination should clamp out-of-range page to last page");
            TestAssert.Equal(2, response.Itens.Count, "Pagination should return the clamped page items");
        }

        public static async Task ValidFiltersWithNoMatchesReturnEmptyMetadata()
        {
            var service = CriarServico(CriarRepositorioComDespesas());

            var response = await service.ConsultarDespesasAsync(Request(idNaturezaDespesa: DespesaTestFixtures.NaturezaPasseioId, idContaFinanceira: DespesaTestFixtures.ContaId));

            TestAssert.Equal(0, response.TotalResultados, "Empty filter should have no results");
            TestAssert.Equal(0, response.TotalPaginas, "Empty filter should have no pages");
            TestAssert.Equal(1, response.PaginaAtual, "Empty filter should stay on page one");
            TestAssert.Equal(0m, response.ValorTotalFiltrado, "Empty filter should have zero total");
            TestAssert.Equal(0, response.Itens.Count, "Empty filter should return no items");
        }

        public static async Task InvalidFiltersReturnValidationError()
        {
            var service = CriarServico(CriarRepositorioComDespesas());

            await TestAssert.ThrowsAsync<ArgumentException>(
                () => service.ConsultarDespesasAsync(Request(mes: 13)),
                "Invalid month should fail");

            await TestAssert.ThrowsAsync<ArgumentException>(
                () => service.ConsultarDespesasAsync(Request(idContaFinanceira: Guid.NewGuid())),
                "Foreign account filter should fail");
        }

        private static ConsultaDespesasRequest Request(
            int mes = 7,
            int ano = 2026,
            Guid? idContaFinanceira = null,
            Guid? idTipoDespesa = null,
            Guid? idNaturezaDespesa = null,
            int pagina = 1,
            int tamanhoPagina = 10)
        {
            return new ConsultaDespesasRequest
            {
                Mes = mes,
                Ano = ano,
                IdContaFinanceira = idContaFinanceira,
                IdTipoDespesa = idTipoDespesa,
                IdNaturezaDespesa = idNaturezaDespesa,
                Pagina = pagina,
                TamanhoPagina = tamanhoPagina
            };
        }

        private static InMemoryDespesaRepository CriarRepositorioComDespesas()
        {
            return new InMemoryDespesaRepository
            {
                Despesas =
                {
                    new Despesa("Aluguel", 900m, new DateTime(2026, 7, 10), DespesaTestFixtures.ContaId, DespesaTestFixtures.CategoriaAluguelId, "test-user"),
                    new Despesa("Mercado", 200m, new DateTime(2026, 7, 8), DespesaTestFixtures.ContaId, DespesaTestFixtures.NaturezaMercadoId, "test-user"),
                    new Despesa("Cinema", 80m, new DateTime(2026, 7, 6), DespesaTestFixtures.ContaLazerId, DespesaTestFixtures.CategoriaCinemaId, "test-user"),
                    new Despesa("Outro usuario", 999m, new DateTime(2026, 7, 7), DespesaTestFixtures.ContaId, DespesaTestFixtures.CategoriaAluguelId, "user-b"),
                    new Despesa("Agosto", 300m, new DateTime(2026, 8, 1), DespesaTestFixtures.ContaId, DespesaTestFixtures.CategoriaAluguelId, "test-user"),
                    new Despesa("Conta base", 50m, new DateTime(2026, 7, 4), DespesaTestFixtures.ContaId, DespesaTestFixtures.CategoriaId, "test-user")
                }
            };
        }

        private static YourMoney.Application.Services.DespesaService CriarServico(
            InMemoryDespesaRepository despesaRepository,
            InMemoryReceitaRepository? receitaRepository = null)
        {
            return DespesaTestFixtures.CreateService(
                despesaRepository,
                receitaRepository ?? new InMemoryReceitaRepository(),
                categoriaRepository: new CategoriaRepositoryStub(true, DespesaTestFixtures.CategoriasPadrao()),
                contaRepository: new ContaFinanceiraRepositoryStub(true, new[] { DespesaTestFixtures.ContaId, DespesaTestFixtures.ContaLazerId }));
        }
    }
}
