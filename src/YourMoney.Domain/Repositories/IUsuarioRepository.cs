using YourMoney.Domain.Entities.Usuario;

namespace YourMoney.Domain.Repositories
{

    public interface IUsuarioRepository
    {
        Task<(int, IEnumerable<Usuario>)> UsuariosAsync();
        Task<(int, IEnumerable<Usuario>)> UsuariosPorTipoAsync(bool AcessoExterno);
        Task<Usuario> UsuariosPorIdAsync(int idUsuario);
        Task<Usuario> AdicionarUsuarioAsync(Usuario oUsuario);
        Task<Usuario> BuscaUsuarioPorCpfCnpj(string CpfCnpj);
        Task<Usuario> BuscaUsuarioPorEmail(string Email);

        Task<Usuario> LoginUsuarioAsync(string CpfCnpjEmail);
        Task<Usuario> LoginUsuarioPorTokenAsync(string Token);

        Task<bool> AlterarSenhaAsync(string novaSenha, string cpf_cnpf);


    }
}