namespace YourMoney.Domain.Entities
{
    public class TipoDespesa
    {
        public int idTipoDespesa { get; private set; }
        public string txtDescricao { get; private set; } = default!;

        private TipoDespesa() { } // ORM

        public TipoDespesa(int id, string nome)
        {
            idTipoDespesa = id;
            txtDescricao = nome;
        }
    }
}
