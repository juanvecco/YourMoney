namespace YourMoney.Domain.Entities
{
    public class BaseEntity
    {
        public Guid Id { get; protected set; }
        public string UsuarioId { get; private set; } = string.Empty;

        public void DefinirUsuario(string usuarioId)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
                throw new ArgumentException("Usuario e obrigatorio.", nameof(usuarioId));

            UsuarioId = usuarioId;
        }
    }
}
