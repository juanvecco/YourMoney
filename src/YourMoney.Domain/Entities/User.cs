using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourMoney.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; } // Em um projeto real, armazene o hash da senha
        public string Role { get; set; } // Ex: "Admin", "User"
    }
}
