namespace YourMoney.Application.DTOs
{
    public class AuthResult
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; } // Novo campo
        public DateTime Expiration { get; set; }
        public string Username { get; set; }
    }
}