using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Repositories;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;

namespace YourMoney.Application.Services
{
    public class DespesaService : IDespesaService
    {
        private readonly IDespesaRepository _despesaRepository;
        private readonly IReceitaRepository _receitaRepository;
        private readonly IContaFinanceiraRepository _contaFinanceiraRepository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly ICurrentUserService _currentUserService;

        public DespesaService(
            IDespesaRepository despesaRepository,
            IReceitaRepository receitaRepository,
            IContaFinanceiraRepository contaFinanceiraRepository,
            ICategoriaRepository categoriaRepository,
            ICurrentUserService currentUserService)
        {
            _despesaRepository = despesaRepository;
            _receitaRepository = receitaRepository;
            _contaFinanceiraRepository = contaFinanceiraRepository;
            _categoriaRepository = categoriaRepository;
            _currentUserService = currentUserService;
        }

        public async Task AdicionarDespesaAsync(Despesa despesa)
        {
            if (despesa.Valor <= 0)
            {
                throw new ArgumentException("O valor da despesa deve ser maior que zero.");
            }
            despesa.DefinirUsuario(_currentUserService.UserId);
            await _despesaRepository.AdicionarAsync(despesa);
        }

        public async Task<CriarDespesaResponse> CriarDespesaAsync(CriarDespesaRequest request)
        {
            await ValidarCriacaoDespesaAsync(request);

            var usuarioId = _currentUserService.UserId;
            var despesa = new Despesa(
                request.Descricao!,
                decimal.Round(request.Valor, 2, MidpointRounding.AwayFromZero),
                request.Data.Date,
                request.IdContaFinanceira,
                request.IdCategoria);
            despesa.DefinirUsuario(usuarioId);

            await _despesaRepository.AdicionarAsync(despesa);

            return MapearCriacaoDespesa(despesa, request.MesReferencia.Date);
        }

        public async Task<Despesa> GetDespesaByIdAsync(Guid id)
        {
            var despesa = await _despesaRepository.GetByIdAsync(id, _currentUserService.UserId);
            if (despesa == null)
            {
                throw new InvalidOperationException("Despesa não encontrada.");
            }
            return despesa;
        }

        public async Task<DespesaDTO> ObterDtoPorIdAsync(Guid id)
        {
            var despesa = await GetDespesaByIdAsync(id);
            var valorReembolsado = await _receitaRepository.GetTotalReembolsadoPorDespesaAsync(
                despesa.Id,
                _currentUserService.UserId);
            return MapearDespesa(despesa, valorReembolsado);
        }

        public async Task RemoverDespesaAsync(Guid id)
        {
            var despesa = await _despesaRepository.GetByIdAsync(id, _currentUserService.UserId);
            if (despesa == null)
            {
                throw new InvalidOperationException("Despesa não encontrada.");
            }
            await _despesaRepository.RemoverAsync(id, _currentUserService.UserId);
        }

        public async Task AtualizarAsync(Despesa despesa)
        {
            // Verifica se a despesa existe antes de atualizá-la
            var existingDespesa = await _despesaRepository.GetByIdAsync(despesa.Id, _currentUserService.UserId);
            if (existingDespesa == null)
            {
                throw new InvalidOperationException("Despesa não encontrada.");
            }
            despesa.DefinirUsuario(_currentUserService.UserId);
            await _despesaRepository.AtualizarAsync(despesa);
        }

        public async Task<List<Despesa>> ListarAsync()
        {
            return await _despesaRepository.ListarAsync(_currentUserService.UserId);
        }

        public async Task<List<DespesaDTO>> ObterPorMesAnoAsync(int mes, int ano, Guid? idContaFinanceira = null)
        {
            var despesas = await _despesaRepository.ObterPorMesAnoAsync(mes, ano, _currentUserService.UserId, idContaFinanceira);
            var filtradas = despesas.Where(d => d.Data.Month == mes && d.Data.Year == ano
                                        && (idContaFinanceira == null || d.IdContaFinanceira == idContaFinanceira))
                           .ToList();
            var reembolsos = await _receitaRepository.GetTotaisReembolsadosPorDespesasAsync(
                filtradas.Select(d => d.Id).ToList(),
                _currentUserService.UserId);

            return filtradas
                .Select(d => MapearDespesa(d, reembolsos.TryGetValue(d.Id, out var valor) ? valor : 0m))
                .ToList();
        }

        public async Task<ConsultaDespesasResponse> ConsultarDespesasAsync(ConsultaDespesasRequest request)
        {
            await ValidarConsultaDespesasAsync(request);

            var usuarioId = _currentUserService.UserId;
            var pagina = Math.Max(request.Pagina, 1);
            var tamanhoPagina = Math.Clamp(request.TamanhoPagina <= 0 ? 10 : request.TamanhoPagina, 1, 100);
            var idsCategoria = await ResolverIdsCategoriaFiltroAsync(request, usuarioId);

            var despesas = await _despesaRepository.ConsultarAsync(
                request.Mes,
                request.Ano,
                usuarioId,
                request.IdContaFinanceira,
                idsCategoria);

            var reembolsos = await _receitaRepository.GetTotaisReembolsadosPorDespesasAsync(
                despesas.Select(d => d.Id).ToList(),
                usuarioId);

            var itensFiltrados = despesas
                .Select(d => MapearDespesa(d, reembolsos.TryGetValue(d.Id, out var valor) ? valor : 0m))
                .ToList();

            var totalResultados = itensFiltrados.Count;
            var totalPaginas = totalResultados == 0
                ? 0
                : (int)Math.Ceiling(totalResultados / (decimal)tamanhoPagina);

            if (totalPaginas > 0 && pagina > totalPaginas)
                pagina = totalPaginas;

            var itensPagina = totalResultados == 0
                ? new List<DespesaDTO>()
                : itensFiltrados
                    .Skip((pagina - 1) * tamanhoPagina)
                    .Take(tamanhoPagina)
                    .ToList();

            return new ConsultaDespesasResponse
            {
                Itens = itensPagina,
                PaginaAtual = totalResultados == 0 ? 1 : pagina,
                TamanhoPagina = tamanhoPagina,
                TotalResultados = totalResultados,
                TotalPaginas = totalPaginas,
                ValorTotalFiltrado = decimal.Round(
                    itensFiltrados.Sum(d => d.ValorLiquido),
                    2,
                    MidpointRounding.AwayFromZero),
                TotaisPorConta = itensFiltrados
                    .GroupBy(d => d.IdContaFinanceira)
                    .Select(g => new ConsultaDespesasTotalPorContaDTO
                    {
                        IdContaFinanceira = g.Key,
                        Valor = decimal.Round(g.Sum(d => d.ValorLiquido), 2, MidpointRounding.AwayFromZero)
                    })
                    .OrderByDescending(t => t.Valor)
                    .ToList()
            };
        }

        public async Task<ParcelamentoDespesaResponse> CriarParcelamentoAsync(ParcelamentoDespesaRequest request)
        {
            await ValidarParcelamentoAsync(request);
            var usuarioId = _currentUserService.UserId;

            if (request.QuantidadeParcelas == 1)
            {
                var despesa = new Despesa(
                    request.Descricao!,
                    decimal.Round(request.ValorTotal, 2, MidpointRounding.AwayFromZero),
                    request.DataInicial.Date,
                    request.IdContaFinanceira,
                    request.IdCategoria);
                despesa.DefinirUsuario(usuarioId);

                await _despesaRepository.AdicionarAsync(despesa);

                return new ParcelamentoDespesaResponse
                {
                    ParcelamentoId = null,
                    ValorTotal = despesa.Valor,
                    QuantidadeParcelas = 1,
                    Parcelas = new List<ParcelaDespesaResponse> { MapearParcela(despesa) }
                };
            }

            var parcelamentoId = Guid.NewGuid();
            var valores = CalcularValoresParcelas(request.ValorTotal, request.QuantidadeParcelas);
            var despesas = new List<Despesa>(request.QuantidadeParcelas);

            for (var index = 0; index < request.QuantidadeParcelas; index++)
            {
                var despesa = new Despesa(
                    request.Descricao!,
                    valores[index],
                    CalcularDataParcela(request.DataInicial, index),
                    request.IdContaFinanceira,
                    request.IdCategoria);
                despesa.DefinirUsuario(usuarioId);

                despesa.AplicarParcelamento(
                    parcelamentoId,
                    index + 1,
                    request.QuantidadeParcelas,
                    decimal.Round(request.ValorTotal, 2, MidpointRounding.AwayFromZero));

                despesas.Add(despesa);
            }

            if (despesas.Sum(d => d.Valor) != decimal.Round(request.ValorTotal, 2, MidpointRounding.AwayFromZero))
                throw new InvalidOperationException("A soma das parcelas não confere com o valor total.");

            await _despesaRepository.AdicionarEmLoteAsync(despesas);

            return new ParcelamentoDespesaResponse
            {
                ParcelamentoId = parcelamentoId,
                ValorTotal = decimal.Round(request.ValorTotal, 2, MidpointRounding.AwayFromZero),
                QuantidadeParcelas = request.QuantidadeParcelas,
                Parcelas = despesas.Select(MapearParcela).ToList()
            };
        }

        private async Task ValidarParcelamentoAsync(ParcelamentoDespesaRequest request)
        {
            if (request == null)
                throw new ArgumentException("Dados do parcelamento são obrigatórios.");
            if (string.IsNullOrWhiteSpace(request.Descricao))
                throw new ArgumentException("Descrição é obrigatória.");
            if (request.ValorTotal <= 0)
                throw new ArgumentException("Valor total deve ser maior que zero.");
            if (request.QuantidadeParcelas < 1 || request.QuantidadeParcelas > 120)
                throw new ArgumentException("Quantidade de parcelas deve estar entre 1 e 120.");
            if (request.DataInicial == default)
                throw new ArgumentException("Data inicial é obrigatória.");
            if (request.IdContaFinanceira == Guid.Empty)
                throw new ArgumentException("Conta Financeira é obrigatória.");
            if (request.IdCategoria == Guid.Empty)
                throw new ArgumentException("Categoria é obrigatória.");
            var usuarioId = _currentUserService.UserId;
            if (!await _contaFinanceiraRepository.ExisteAsync(request.IdContaFinanceira, usuarioId))
                throw new ArgumentException("Conta Financeira não encontrada.");
            if (!await _categoriaRepository.ExisteAsync(request.IdCategoria, usuarioId))
                throw new ArgumentException("Categoria não encontrada.");
        }

        private async Task ValidarCriacaoDespesaAsync(CriarDespesaRequest request)
        {
            if (request == null)
                throw new ArgumentException("Dados da despesa são obrigatórios.");
            if (string.IsNullOrWhiteSpace(request.Descricao))
                throw new ArgumentException("Descrição é obrigatória.");
            if (request.Valor <= 0)
                throw new ArgumentException("Valor deve ser maior que zero.");
            if (request.Data == default)
                throw new ArgumentException("Data é obrigatória.");
            if (request.MesReferencia == default)
                throw new ArgumentException("Mês de referência é obrigatório.");
            if (request.IdContaFinanceira == Guid.Empty)
                throw new ArgumentException("Conta Financeira é obrigatória.");
            if (request.IdCategoria == Guid.Empty)
                throw new ArgumentException("Categoria é obrigatória.");

            var usuarioId = _currentUserService.UserId;
            if (!await _contaFinanceiraRepository.ExisteAsync(request.IdContaFinanceira, usuarioId))
                throw new ArgumentException("Conta Financeira não encontrada.");
            if (!await _categoriaRepository.ExisteAsync(request.IdCategoria, usuarioId))
                throw new ArgumentException("Categoria não encontrada.");
        }

        private async Task ValidarConsultaDespesasAsync(ConsultaDespesasRequest request)
        {
            if (request == null)
                throw new ArgumentException("Filtros de despesa inválidos.");
            if (request.Mes < 1 || request.Mes > 12)
                throw new ArgumentException("Filtros de despesa inválidos.");
            if (request.Ano < 1)
                throw new ArgumentException("Filtros de despesa inválidos.");
            if (request.Pagina < 1)
                throw new ArgumentException("Filtros de despesa inválidos.");
            if (request.TamanhoPagina < 1 || request.TamanhoPagina > 100)
                throw new ArgumentException("Filtros de despesa inválidos.");

            var usuarioId = _currentUserService.UserId;
            if (request.IdContaFinanceira.HasValue
                && !await _contaFinanceiraRepository.ExisteAsync(request.IdContaFinanceira.Value, usuarioId))
                throw new ArgumentException("Filtros de despesa inválidos.");
        }

        private async Task<IReadOnlyCollection<Guid>> ResolverIdsCategoriaFiltroAsync(
            ConsultaDespesasRequest request,
            string usuarioId)
        {
            if (!request.IdTipoDespesa.HasValue && !request.IdNaturezaDespesa.HasValue)
                return null!;

            var categorias = await _categoriaRepository.GetAllAsync(usuarioId);
            var categoriasDespesa = categorias
                .Where(c => c.TipoTransacao == TipoTransacao.Despesa || c.TipoTransacao == TipoTransacao.Receita)
                .ToList();

            var filtros = new List<HashSet<Guid>>();

            if (request.IdTipoDespesa.HasValue)
            {
                var tipo = categorias.FirstOrDefault(c => c.Id == request.IdTipoDespesa.Value)
                    ?? throw new ArgumentException("Filtros de despesa inválidos.");
                filtros.Add(ObterCategoriaEDescendentes(categoriasDespesa, tipo.Id));
            }

            if (request.IdNaturezaDespesa.HasValue)
            {
                var natureza = categorias.FirstOrDefault(c => c.Id == request.IdNaturezaDespesa.Value)
                    ?? throw new ArgumentException("Filtros de despesa inválidos.");
                filtros.Add(ObterCategoriaEDescendentes(categoriasDespesa, natureza.Id));
            }

            if (filtros.Count == 0)
                return null!;

            var ids = filtros[0];
            foreach (var filtro in filtros.Skip(1))
                ids.IntersectWith(filtro);

            return ids;
        }

        private static HashSet<Guid> ObterCategoriaEDescendentes(List<Categoria> categorias, Guid raizId)
        {
            var ids = new HashSet<Guid> { raizId };
            var adicionou = true;

            while (adicionou)
            {
                adicionou = false;
                foreach (var categoria in categorias)
                {
                    if (categoria.CategoriaPaiId.HasValue
                        && ids.Contains(categoria.CategoriaPaiId.Value)
                        && ids.Add(categoria.Id))
                    {
                        adicionou = true;
                    }
                }
            }

            return ids;
        }

        private static IReadOnlyList<decimal> CalcularValoresParcelas(decimal valorTotal, int quantidadeParcelas)
        {
            var totalCentavos = decimal.ToInt64(decimal.Round(valorTotal * 100, 0, MidpointRounding.AwayFromZero));
            var valorBase = totalCentavos / quantidadeParcelas;
            var resto = totalCentavos % quantidadeParcelas;
            var valores = new List<decimal>(quantidadeParcelas);

            for (var index = 0; index < quantidadeParcelas; index++)
            {
                var centavos = valorBase + (index < resto ? 1 : 0);
                valores.Add(centavos / 100m);
            }

            return valores;
        }

        private static DateTime CalcularDataParcela(DateTime dataInicial, int indiceParcela)
        {
            var mesAlvo = new DateTime(dataInicial.Year, dataInicial.Month, 1).AddMonths(indiceParcela);
            var dia = Math.Min(dataInicial.Day, DateTime.DaysInMonth(mesAlvo.Year, mesAlvo.Month));
            return new DateTime(mesAlvo.Year, mesAlvo.Month, dia);
        }

        private static DespesaDTO MapearDespesa(Despesa despesa, decimal valorReembolsado = 0m)
        {
            var valorReembolsadoArredondado = decimal.Round(valorReembolsado, 2, MidpointRounding.AwayFromZero);
            var valorLiquido = decimal.Round(despesa.Valor - valorReembolsadoArredondado, 2, MidpointRounding.AwayFromZero);
            if (valorLiquido < 0)
                valorLiquido = 0m;

            return new DespesaDTO
            {
                Id = despesa.Id,
                Descricao = despesa.Descricao,
                Valor = despesa.Valor,
                Data = despesa.Data,
                IdContaFinanceira = despesa.IdContaFinanceira,
                IdCategoria = despesa.IdCategoria,
                ParcelamentoId = despesa.ParcelamentoId,
                NumeroParcela = despesa.NumeroParcela,
                TotalParcelas = despesa.TotalParcelas,
                ValorTotalParcelamento = despesa.ValorTotalParcelamento,
                ValorReembolsado = valorReembolsadoArredondado,
                ValorLiquido = valorLiquido,
                PossuiReembolso = valorReembolsadoArredondado > 0
            };
        }

        private static CriarDespesaResponse MapearCriacaoDespesa(Despesa despesa, DateTime mesReferencia)
        {
            return new CriarDespesaResponse
            {
                Id = despesa.Id,
                Descricao = despesa.Descricao,
                Valor = despesa.Valor,
                Data = despesa.Data,
                MesReferencia = mesReferencia,
                IdContaFinanceira = despesa.IdContaFinanceira,
                IdCategoria = despesa.IdCategoria
            };
        }

        private static ParcelaDespesaResponse MapearParcela(Despesa despesa)
        {
            var dto = MapearDespesa(despesa);
            return new ParcelaDespesaResponse
            {
                Id = dto.Id,
                Descricao = dto.Descricao,
                Valor = dto.Valor,
                Data = dto.Data,
                IdContaFinanceira = dto.IdContaFinanceira,
                IdCategoria = dto.IdCategoria,
                ParcelamentoId = dto.ParcelamentoId,
                NumeroParcela = dto.NumeroParcela,
                TotalParcelas = dto.TotalParcelas,
                ValorTotalParcelamento = dto.ValorTotalParcelamento
            };
        }

    }
}
