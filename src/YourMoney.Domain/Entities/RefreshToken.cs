using System.ComponentModel.DataAnnotations;

namespace YourMoney.Domain.Entities
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public string CreatedByIp { get; set; }
        public DateTime? Revoked { get; set; }
        public string RevokedByIp { get; set; }
        public string ReplacedByToken { get; set; }
        public string UserId { get; set; } // Chave estrangeira para o usuário do Identity
        public bool IsActive => Revoked == null && Expires >= DateTime.UtcNow;
    }
}