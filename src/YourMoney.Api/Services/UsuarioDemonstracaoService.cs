using Microsoft.EntityFrameworkCore;
using YourMoney.Application.Services;
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

        var wallet = await _dbContext.Carteiras.SingleOrDefaultAsync(
            item => item.UsuarioId == usuarioId && item.Principal,
            cancellationToken);
        if (wallet is null)
        {
            wallet = new Carteira(usuarioId, "Minha carteira", principal: true);
            _dbContext.Carteiras.Add(wallet);
        }
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

        _dbContext.RegrasNegocioUsuario.AddRange(
            RegraNegocioCatalogo.Todas.Select(rule => new RegraNegocioUsuario(usuarioId, rule.Codigo, ativa: false)));

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static Categoria CriarCategoria(
        string descricao,
        TipoTransacao tipo,
        string usuarioId,
        Guid? categoriaPaiId = null) => new(descricao, tipo, usuarioId, categoriaPaiId);
}
