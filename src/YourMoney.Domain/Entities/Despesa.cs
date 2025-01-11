namespace YourMoney.Domain.Entities
{
    public class Despesa
    {
        public Guid Id { get; private set; }
        public string Descricao { get; private set; }
        public decimal Valor { get; private set; }
        public DateTime Data { get; private set; }
        public string Categoria { get; private set; }

        private Despesa() { } // Para ORM

        public Despesa(string descricao, decimal valor, DateTime data, string categoria)
        {
            Id = Guid.NewGuid();
            Descricao = descricao;
            Valor = valor;
            Data = data;
            Categoria = categoria;

            Validate();
        }

        private void Validate()
        {
            if (string.IsNullOrEmpty(Descricao))
                throw new ArgumentException("Descrição não pode ser vazia.");
            if (Valor <= 0)
                throw new ArgumentException("O valor da despesa deve ser maior que zero.");
            //if (Data > DateTime.Now)
                //throw new ArgumentException("A data da despesa não pode estar no futuro.");
            if (string.IsNullOrEmpty(Categoria))
                throw new ArgumentException("Categoria não pode ser vazia.");
        }
    }

}
