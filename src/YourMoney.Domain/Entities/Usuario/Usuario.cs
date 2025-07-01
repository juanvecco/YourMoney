using System.ComponentModel.DataAnnotations;

namespace YourMoney.Domain.Entities.Usuario
{
    public class Usuario
    {
        public Usuario() { }
        #region Properties
        [Key]
        public int idUsuario { get; set; }

        public string txtCpfCnpj { get; set; }

        public string txtNome { get; set; }

        public string txtEmail { get; set; }

        public string txtSenha { get; set; }

        public bool blnAlterarSenha { get; set; }

        public bool blnExcluido { get; set; }
        #endregion
    }
}