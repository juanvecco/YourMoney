using System.Globalization;
using System.Text;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;

namespace YourMoney.Application.Services;

public sealed class MovimentacaoFinanceiraService : IMovimentacaoFinanceiraService
{
    private static readonly HashSet<string> TiposPermitidos =
        new(StringComparer.OrdinalIgnoreCase) { "receita", "despesa", "investimento" };

    private readonly IReceitaRepository _receitas;
    private readonly IDespesaRepository _despesas;
    private readonly IInvestimentoRepository _investimentos;
    private readonly IContaFinanceiraRepository _contas;
    private readonly ICurrentUserService _currentUser;

    public MovimentacaoFinanceiraService(
        IReceitaRepository receitas,
        IDespesaRepository despesas,
        IInvestimentoRepository investimentos,
        IContaFinanceiraRepository contas,
        ICurrentUserService currentUser)
    {
        _receitas = receitas;
        _despesas = despesas;
        _investimentos = investimentos;
        _contas = contas;
        _currentUser = currentUser;
    }

    public async Task<ConsultaMovimentacoesResponse> ConsultarAsync(ConsultaMovimentacoesRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var inicio = LerMes(request.MesInicial, nameof(request.MesInicial));
        var ultimoMes = LerMes(request.MesFinal, nameof(request.MesFinal));
        var fimExclusivo = ultimoMes.AddMonths(1);

        if (ultimoMes < inicio)
            throw new ArgumentException("O mês final deve ser igual ou posterior ao mês inicial.");
        if (fimExclusivo > inicio.AddMonths(12))
            throw new ArgumentException("O intervalo máximo de consulta é de 12 meses.");
        if (request.Pagina < 1)
            throw new ArgumentException("A página deve ser maior ou igual a 1.");
        if (request.TamanhoPagina is < 10 or > 100)
            throw new ArgumentException("O tamanho da página deve estar entre 10 e 100.");
        if (request.Busca?.Trim().Length > 100)
            throw new ArgumentException("A busca deve ter no máximo 100 caracteres.");

        var tipo = request.Tipo?.Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(tipo) && !TiposPermitidos.Contains(tipo))
            throw new ArgumentException("O tipo de movimentação é inválido.");

        var usuarioId = _currentUser.UserId;
        if (request.ContaFinanceiraId.HasValue
            && !await _contas.ExisteAsync(request.ContaFinanceiraId.Value, usuarioId))
        {
            throw new ArgumentException("A conta financeira informada não foi encontrada.");
        }

        // Os repositórios compartilham o AppDbContext scoped; EF Core não permite
        // executar operações concorrentes sobre a mesma instância.
        var receitas = await _receitas.GetByReferenciaPeriodoAsync(inicio, fimExclusivo, usuarioId);
        var despesas = await _despesas.GetByReferenciaPeriodoAsync(inicio, fimExclusivo, usuarioId);
        var investimentos = await _investimentos.GetByReferenciaPeriodoAsync(inicio, fimExclusivo, usuarioId);

        var itens = new List<MovimentacaoFinanceiraDto>();
        itens.AddRange(receitas.Select(Mapear));
        itens.AddRange(despesas.Select(Mapear));
        itens.AddRange(investimentos.Select(Mapear));

        IEnumerable<MovimentacaoFinanceiraDto> filtrados = itens;
        if (!string.IsNullOrEmpty(tipo))
            filtrados = filtrados.Where(item => item.Tipo == tipo);
        if (request.ContaFinanceiraId.HasValue)
            filtrados = filtrados.Where(item => item.ContaFinanceiraId == request.ContaFinanceiraId);

        var busca = Normalizar(request.Busca);
        if (!string.IsNullOrEmpty(busca))
        {
            filtrados = filtrados.Where(item =>
                Normalizar(item.Descricao).Contains(busca, StringComparison.Ordinal)
                || Normalizar(item.ContaFinanceiraNome).Contains(busca, StringComparison.Ordinal)
                || Normalizar(item.Categoria).Contains(busca, StringComparison.Ordinal));
        }

        var materializados = filtrados
            .OrderByDescending(item => item.Data)
            .ThenBy(item => item.Tipo, StringComparer.Ordinal)
            .ThenBy(item => item.Id)
            .ToList();

        var totais = new ResumoFinanceiroPeriodoDto
        {
            Receitas = materializados.Where(item => item.Tipo == "receita").Sum(item => item.Valor),
            Despesas = materializados.Where(item => item.Tipo == "despesa").Sum(item => item.Valor),
            Investimentos = materializados.Where(item => item.Tipo == "investimento").Sum(item => item.Valor)
        };
        var totaisPorMes = materializados
            .GroupBy(item => new DateTime(item.MesReferencia.Year, item.MesReferencia.Month, 1))
            .OrderBy(grupo => grupo.Key)
            .Select(grupo => new ResumoFinanceiroMensalDto
            {
                MesReferencia = grupo.Key.ToString("yyyy-MM", CultureInfo.InvariantCulture),
                TotalItens = grupo.Count(),
                Receitas = grupo.Where(item => item.Tipo == "receita").Sum(item => item.Valor),
                Despesas = grupo.Where(item => item.Tipo == "despesa").Sum(item => item.Valor),
                Investimentos = grupo.Where(item => item.Tipo == "investimento").Sum(item => item.Valor)
            })
            .ToList();
        var totalPaginas = materializados.Count == 0
            ? 0
            : (int)Math.Ceiling(materializados.Count / (decimal)request.TamanhoPagina);

        return new ConsultaMovimentacoesResponse
        {
            Itens = materializados
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToList(),
            Pagina = request.Pagina,
            TamanhoPagina = request.TamanhoPagina,
            TotalItens = materializados.Count,
            TotalPaginas = totalPaginas,
            Totais = totais,
            TotaisPorMes = totaisPorMes
        };
    }

    private static DateTime LerMes(string value, string field) =>
        DateTime.TryParseExact(value?.Trim(), "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var month)
            ? new DateTime(month.Year, month.Month, 1)
            : throw new ArgumentException($"O campo {field} deve usar o formato AAAA-MM.");

    private static MovimentacaoFinanceiraDto Mapear(Receita item) => new()
    {
        Id = item.Id, Tipo = "receita", Descricao = item.Descricao, Data = item.Data,
        MesReferencia = item.MesReferencia ?? new DateTime(item.Data.Year, item.Data.Month, 1),
        Valor = item.Valor, ContaFinanceiraId = item.IdContaFinanceira,
        ContaFinanceiraNome = item.ContaFinanceira?.Descricao, RotaOrigem = "/receitas"
    };

    private static MovimentacaoFinanceiraDto Mapear(Despesa item) => new()
    {
        Id = item.Id, Tipo = "despesa", Descricao = item.Descricao, Data = item.Data,
        MesReferencia = new DateTime(item.Data.Year, item.Data.Month, 1), Valor = item.Valor,
        ContaFinanceiraId = item.IdContaFinanceira, ContaFinanceiraNome = item.ContaFinanceira?.Descricao,
        Categoria = item.Categoria?.Descricao, RotaOrigem = "/despesas"
    };

    private static MovimentacaoFinanceiraDto Mapear(Investimento item) => new()
    {
        Id = item.Id, Tipo = "investimento", Descricao = item.Nome, Data = item.DataInvestimento,
        MesReferencia = item.MesReferencia ?? new DateTime(item.DataInvestimento.Year, item.DataInvestimento.Month, 1),
        Valor = item.ValorAtual, RotaOrigem = "/investimento"
    };

    private static string Normalizar(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var decomposed = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        return new string(decomposed.Where(c =>
            CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray());
    }
}
