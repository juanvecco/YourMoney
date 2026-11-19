using System.ComponentModel.DataAnnotations;

namespace YourMoney.Api.Models
{
    public class UsuarioRegistro
    {
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Informe um nome de 2 a 100 caracteres")]
        public string Nome { get; set; }

        [RegularExpression("^(gastos|cartao|objetivos|saldo)$", ErrorMessage = "Escolha um objetivo válido")]
        public string ObjetivoInicial { get; set; } = "gastos";

        public List<string>? ObjetivosIniciais { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [EmailAddress(ErrorMessage = "O campo {0} está em formato inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [StringLength(100, ErrorMessage = "O campo {0} precisa ter entre {2} e {1} caracteres", MinimumLength = 6)]
        public string Senha { get; set; }

        [Compare("Senha", ErrorMessage = "As senhas não conferem.")]
        public string SenhaConfirmacao { get; set; }

        [StringLength(120, ErrorMessage = "A origem deve ter até {1} caracteres")]
        [RegularExpression("^(instagram|google-play|amigo-familiar|tiktok|google|facebook|youtube|linkedin|outro|prefiro-nao-responder)$", ErrorMessage = "Escolha uma origem válida")]
        public string OrigemConhecimento { get; set; }

        public string CodigoConvite { get; set; }
    }

    public class UsuarioLogin
    {
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [EmailAddress(ErrorMessage = "O campo {0} está em formato inválido")]
        public string Email { get; set; }
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public string Senha { get; set; }
    }

    public sealed class CadastroExternoInicio
    {
        [StringLength(100, MinimumLength = 2)]
        public string Nome { get; set; }

        [RegularExpression("^(gastos|cartao|objetivos|saldo)$")]
        public string ObjetivoInicial { get; set; }

        public List<string>? ObjetivosIniciais { get; set; }

        [RegularExpression("^(instagram|google-play|amigo-familiar|tiktok|google|facebook|youtube|linkedin|outro|prefiro-nao-responder)$")]
        public string OrigemConhecimento { get; set; }

        public string CodigoConvite { get; set; }

        public bool ModoEntrar { get; set; }
    }

    public class UsuarioEsqueceuSenha
    {
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [EmailAddress(ErrorMessage = "O campo {0} está em formato inválido")]
        public string Email { get; set; }
    }

    public class UsuarioResetSenha
    {
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [EmailAddress(ErrorMessage = "O campo {0} está em formato inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public string Token { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [StringLength(100, ErrorMessage = "O campo {0} precisa ter entre {2} e {1} caracteres", MinimumLength = 6)]
        public string Senha { get; set; }

        [Compare("Senha", ErrorMessage = "As senhas não conferem.")]
        public string SenhaConfirmacao { get; set; }
    }

    public class UsuarioRespostaLogin
    {
        public string AccessToken { get; set; }
        public double ExpiresIn { get; set; }
        public UsuarioToken UsuarioToken { get; set; }
    }

    public class UsuarioToken
    {
        public string Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public IEnumerable<string> Roles { get; set; } = [];
        public IEnumerable<UsuarioClaim> Claims { get; set; }
    }
    public class UsuarioClaim
    {
        public string Value { get; set; }
        public string Type { get; set; }
    }
}
