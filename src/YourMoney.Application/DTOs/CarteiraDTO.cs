namespace YourMoney.Application.DTOs;

public sealed class CarteiraRequestDTO
{
    public string Nome { get; set; } = string.Empty;
}

public sealed class CarteiraResponseDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Principal { get; set; }
}

public sealed class CarteiraNaoEncontradaException : InvalidOperationException
{
    public CarteiraNaoEncontradaException()
        : base("Carteira não encontrada.")
    {
    }
}

public sealed class CarteiraNomeDuplicadoException : InvalidOperationException
{
    public CarteiraNomeDuplicadoException()
        : base("Já existe uma carteira com este nome.")
    {
    }
}

public sealed class CarteiraComDependenciasException : InvalidOperationException
{
    public CarteiraComDependenciasException()
        : base("Remova ou transfira as contas e conexões da carteira antes de excluí-la.")
    {
    }
}

public sealed class CarteiraPrincipalException : InvalidOperationException
{
    public CarteiraPrincipalException()
        : base("A carteira principal não pode ser excluída.")
    {
    }
}
