using YourMoney.Domain.Enums;

namespace YourMoney.Application.DTOs
{
    public class CategoriaDTO
    {
        public Guid Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public TipoTransacao TipoTransacao { get; set; }
        public Guid? CategoriaPaiId { get; set; }
    }
}
