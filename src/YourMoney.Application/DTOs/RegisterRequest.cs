using System.ComponentModel.DataAnnotations;

namespace YourMoney.Application.DTOs
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "A senha e a confirmação de senha não coincidem.")]
        public string ConfirmPassword { get; set; }
    }
}