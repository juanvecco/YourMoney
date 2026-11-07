namespace YourMoney.Domain.Entities
{
    public class ContaFinanceira : BaseEntity
    {
        public Guid CarteiraId { get; private set; }
        public string Descricao { get; private set; }          // Ex: Nubank, Dinheiro
        //public string Tipo { get; private set; }          // Ex: Cartão de Crédito, Carteira, Pix
        //public bool Ativa { get; private set; }
        public DateTime DataCriacao { get; private set; }


        private ContaFinanceira() { } // Para ORM

        public ContaFinanceira(string descricao)
        {
            Id = Guid.NewGuid();
            AtualizarDescricao(descricao);
            //AtualizarTipo(tipo);
            //Ativa = true;
            DataCriacao = DateTime.Now;
        }

        public ContaFinanceira(string descricao, string usuarioId)
            : this(descricao)
        {
            DefinirUsuario(usuarioId);
        }

        public ContaFinanceira(string descricao, string usuarioId, Guid carteiraId)
            : this(descricao, usuarioId)
        {
            AssociarCarteira(carteiraId);
        }

        public void AtualizarDescricao(string descricao)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                throw new ArgumentException("Descrição é obrigatória", nameof(descricao));
            Descricao = descricao.Trim();
        }

        public void AssociarCarteira(Guid carteiraId)
        {
            if (carteiraId == Guid.Empty)
                throw new ArgumentException("A carteira é obrigatória.", nameof(carteiraId));
            CarteiraId = carteiraId;
        }

        //public void AtualizarTipo(string tipo)
        //{
        //    if (string.IsNullOrWhiteSpace(tipo))
        //        throw new ArgumentException("Tipo é obrigatório", nameof(tipo));
        //    Tipo = tipo.Trim();
        //}

        //public void Desativar() => Ativa = false;
        //public void Reativar() => Ativa = true;
    }
}
