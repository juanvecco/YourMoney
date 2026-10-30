using YourMoney.Application.DTOs;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;

namespace YourMoney.Tests.Fixtures;

public static class InvestimentoOrganizacaoTestFixtures
{
    public static GrupoObjetivoInvestimento Grupo(string usuario = "user-a", string nome = "Reserva de emergência") =>
        new(nome, usuario);

    public static Investimento Investimento(
        string usuario = "user-a",
        decimal valor = 100m,
        GrupoObjetivoInvestimento? grupo = null,
        TipoAtivo? tipoAtivo = null,
        string tipoLegado = "Tesouro Selic")
    {
        var investimento = new Investimento(
            "Tesouro", "Reserva", tipoLegado, 1m, 100m, valor,
            new DateTime(2026, 8, 1), new DateTime(2026, 8, 1), usuario,
            grupoObjetivoInvestimentoId: grupo?.Id, tipoAtivo: tipoAtivo);
        investimento.AssociarGrupoObjetivo(grupo);
        return investimento;
    }

    public static FiltroOrganizacaoInvestimentos Filtro(
        Guid? grupo = null,
        bool semGrupo = false,
        string? tipo = null,
        bool naoClassificado = false) => new()
        {
            GrupoObjetivoId = grupo,
            SemGrupo = semGrupo,
            TipoAtivo = tipo,
            NaoClassificado = naoClassificado
        };
}
