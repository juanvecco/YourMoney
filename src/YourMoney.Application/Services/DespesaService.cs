using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Repositories;
using YourMoney.Domain.Entities;

namespace YourMoney.Application.Services
{
    public class DespesaService : IDespesaService
    {
        private readonly IDespesaRepository _despesaRepository;
        private readonly IContaFinanceiraRepository _contaFinanceiraRepository;
        private readonly ICategoriaRepository _categoriaRepository;

        public DespesaService(
            IDespesaRepository despesaRepository,
            IContaFinanceiraRepository contaFinanceiraRepository,
            ICategoriaRepository categoriaRepository)
        {
            _despesaRepository = despesaRepository;
            _contaFinanceiraRepository = contaFinanceiraRepository;
            _categoriaRepository = categoriaRepository;
        }

        public async Task AdicionarDespesaAsync(Despesa despesa)
        {
            if (despesa.Valor <= 0)
            {
                throw new ArgumentException("O valor da despesa deve ser maior que zero.");
            }
            await _despesaRepository.AdicionarAsync(despesa);
        }

        public async Task<Despesa> GetDespesaByIdAsync(Guid id)
        {
            var despesa = await _despesaRepository.GetByIdAsync(id);
            if (despesa == null)
            {
                throw new InvalidOperationException("Despesa não encontrada.");
            }
            return despesa;
        }

        public async Task RemoverDespesaAsync(Guid id)
        {
            var despesa = await _despesaRepository.GetByIdAsync(id);
            if (despesa == null)
            {
                throw new InvalidOperationException("Despesa não encontrada.");
            }
            await _despesaRepository.RemoverAsync(id);
        }

        public async Task AtualizarAsync(Despesa despesa)
        {
            // Verifica se a despesa existe antes de atualizá-la
            var existingDespesa = await _despesaRepository.GetByIdAsync(despesa.Id);
            if (existingDespesa == null)
            {
                throw new InvalidOperationException("Despesa não encontrada.");
            }
            await _despesaRepository.AtualizarAsync(despesa);
        }

        public async Task<List<Despesa>> ListarAsync()
        {
            return await _despesaRepository.ListarAsync();
        }

        public async Task<List<DespesaDTO>> ObterPorMesAnoAsync(int mes, int ano, Guid? idContaFinanceira = null)
        {
            var despesas = await _despesaRepository.ObterPorMesAnoAsync(mes, ano, idContaFinanceira);
            return despesas.Where(d => d.Data.Month == mes && d.Data.Year == ano
                                        && (idContaFinanceira == null || d.IdContaFinanceira == idContaFinanceira))
                           .Select(MapearDespesa)
                           .ToList();
        }

        public async Task<ParcelamentoDespesaResponse> CriarParcelamentoAsync(ParcelamentoDespesaRequest request)
        {
            await ValidarParcelamentoAsync(request);

            if (request.QuantidadeParcelas == 1)
            {
                var despesa = new Despesa(
                    request.Descricao!,
                    decimal.Round(request.ValorTotal, 2, MidpointRounding.AwayFromZero),
                    request.DataInicial.Date,
                    request.IdContaFinanceira,
                    request.IdCategoria);

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
            if (!await _contaFinanceiraRepository.ExisteAsync(request.IdContaFinanceira))
                throw new ArgumentException("Conta Financeira não encontrada.");
            if (!await _categoriaRepository.ExisteAsync(request.IdCategoria))
                throw new ArgumentException("Categoria não encontrada.");
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

        private static DespesaDTO MapearDespesa(Despesa despesa)
        {
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
                ValorTotalParcelamento = despesa.ValorTotalParcelamento
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
