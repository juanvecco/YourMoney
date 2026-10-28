using Microsoft.EntityFrameworkCore;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;
using YourMoney.Infrastructure.Persistence;

namespace YourMoney.Api.Services;

public sealed class UsuarioDemonstracaoService : IUsuarioDemonstracaoService
{
    private readonly AppDbContext _dbContext;

    public UsuarioDemonstracaoService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CriarDadosIniciaisAsync(string usuarioId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(usuarioId))
            throw new ArgumentException("O usuário é obrigatório.", nameof(usuarioId));

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var jaPossuiDados = await _dbContext.ContaFinanceira.AnyAsync(c => c.UsuarioId == usuarioId, cancellationToken)
            || await _dbContext.Categorias.AnyAsync(c => c.UsuarioId == usuarioId, cancellationToken)
            || await _dbContext.Despesas.AnyAsync(d => d.UsuarioId == usuarioId, cancellationToken)
            || await _dbContext.Receitas.AnyAsync(r => r.UsuarioId == usuarioId, cancellationToken);

        if (jaPossuiDados)
        {
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        var contaPrincipal = new ContaFinanceira("Nubank (exemplo)", usuarioId);
        var carteira = new ContaFinanceira("Carteira (exemplo)", usuarioId);
        _dbContext.ContaFinanceira.AddRange(contaPrincipal, carteira);

        // A interface existente usa o valor 1 para a árvore de categorias de despesas.
        // O backend aceita os valores 1 e 2 ao validar e consultar essas categorias.
        const TipoTransacao tipoCategoriaDespesa = TipoTransacao.Receita;
        var essenciais = CriarCategoria("Essenciais", tipoCategoriaDespesa, usuarioId);
        var naoEssenciais = CriarCategoria("Não essenciais", tipoCategoriaDespesa, usuarioId);

        var moradia = CriarCategoria("Moradia", tipoCategoriaDespesa, usuarioId, essenciais.Id);
        var alimentacao = CriarCategoria("Alimentação", tipoCategoriaDespesa, usuarioId, essenciais.Id);
        var transporte = CriarCategoria("Transporte", tipoCategoriaDespesa, usuarioId, essenciais.Id);
        var lazer = CriarCategoria("Lazer", tipoCategoriaDespesa, usuarioId, naoEssenciais.Id);
        var assinaturas = CriarCategoria("Assinaturas", tipoCategoriaDespesa, usuarioId, naoEssenciais.Id);

        var energia = CriarCategoria("Energia elétrica", tipoCategoriaDespesa, usuarioId, moradia.Id);
        var aluguel = CriarCategoria("Aluguel", tipoCategoriaDespesa, usuarioId, moradia.Id);
        var mercado = CriarCategoria("Mercado", tipoCategoriaDespesa, usuarioId, alimentacao.Id);
        var restaurante = CriarCategoria("Restaurante", tipoCategoriaDespesa, usuarioId, alimentacao.Id);
        var combustivel = CriarCategoria("Combustível", tipoCategoriaDespesa, usuarioId, transporte.Id);
        var passeios = CriarCategoria("Passeios", tipoCategoriaDespesa, usuarioId, lazer.Id);
        var streaming = CriarCategoria("Streaming", tipoCategoriaDespesa, usuarioId, assinaturas.Id);

        _dbContext.Categorias.AddRange(
            essenciais, naoEssenciais,
            moradia, alimentacao, transporte, lazer, assinaturas,
            energia, aluguel, mercado, restaurante, combustivel, passeios, streaming);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var hoje = DateTime.Today;
        var primeiroDiaDoMes = new DateTime(hoje.Year, hoje.Month, 1);
        _dbContext.Receitas.Add(new Receita(
            "Exemplo: salário mensal", 5000m, hoje, primeiroDiaDoMes, usuarioId,
            NaturezaReceita.RendaDisponivel, idContaFinanceira: contaPrincipal.Id));

        _dbContext.Despesas.AddRange(
            new Despesa("Exemplo: compras do mercado", 420.50m, hoje, contaPrincipal.Id, mercado.Id, usuarioId),
            new Despesa("Exemplo: conta de energia", 135m, hoje, contaPrincipal.Id, energia.Id, usuarioId),
            new Despesa("Exemplo: assinatura de streaming", 39.90m, hoje, carteira.Id, streaming.Id, usuarioId));

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static Categoria CriarCategoria(
        string descricao,
        TipoTransacao tipo,
        string usuarioId,
        Guid? categoriaPaiId = null) => new(descricao, tipo, usuarioId, categoriaPaiId);
}
