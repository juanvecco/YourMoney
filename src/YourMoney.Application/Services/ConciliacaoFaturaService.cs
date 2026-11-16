using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;

namespace YourMoney.Application.Services;

public sealed class ConciliacaoFaturaService : IConciliacaoFaturaService
{
    private readonly IFaturaContaRepository _faturas;
    private readonly IDespesaRepository _despesas;
    private readonly IContaFinanceiraRepository _contas;
    private readonly ICurrentUserService _currentUser;

    public ConciliacaoFaturaService(IFaturaContaRepository faturas, IDespesaRepository despesas,
        IContaFinanceiraRepository contas, ICurrentUserService currentUser)
    {
        _faturas = faturas;
        _despesas = despesas;
        _contas = contas;
        _currentUser = currentUser;
    }

    public async Task<ResumoFaturaResponse> ObterAsync(Guid contaId, int mes, int ano)
    {
        var referencia = ValidarMes(mes, ano);
        var usuarioId = _currentUser.UserId;
        if (contaId == Guid.Empty || !await _contas.ExisteAsync(contaId, usuarioId))
            throw new InvalidOperationException("Conta não encontrada.");

        var fatura = await _faturas.ObterAsync(contaId, referencia, usuarioId);
        var despesas = await _despesas.ObterPorMesAnoAsync(mes, ano, usuarioId, contaId);
        var total = despesas.Sum(d => d.Valor);
        return new ResumoFaturaResponse
        {
            IdContaFinanceira = contaId,
            Mes = mes,
            Ano = ano,
            ValorInformado = fatura?.ValorInformado,
            TotalCadastrado = total,
            Diferenca = fatura?.ValorInformado - total,
            QuantidadeDespesas = despesas.Count,
            QuantidadePendentes = despesas.Count(d => !d.Conferida)
        };
    }

    public async Task<ResumoFaturaResponse> SalvarAsync(Guid contaId, int mes, int ano, decimal valorInformado)
    {
        var referencia = ValidarMes(mes, ano);
        if (valorInformado < 0 || decimal.Round(valorInformado, 2) != valorInformado)
            throw new ArgumentException("O valor da fatura deve ser não negativo e ter até dois centavos.");
        var usuarioId = _currentUser.UserId;
        if (contaId == Guid.Empty || !await _contas.ExisteAsync(contaId, usuarioId))
            throw new InvalidOperationException("Conta não encontrada.");

        var fatura = await _faturas.ObterAsync(contaId, referencia, usuarioId);
        if (fatura is null)
            fatura = new FaturaConta(contaId, referencia, valorInformado, usuarioId);
        else
            fatura.AtualizarValor(valorInformado);
        await _faturas.SalvarAsync(fatura);
        return await ObterAsync(contaId, mes, ano);
    }

    private static DateTime ValidarMes(int mes, int ano)
    {
        if (mes is < 1 or > 12 || ano is < 1 or > 9999)
            throw new ArgumentException("Mês ou ano inválido.");
        return new DateTime(ano, mes, 1);
    }
}
