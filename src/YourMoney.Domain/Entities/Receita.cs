using System;
using YourMoney.Domain.ValueObjects;
using YourMoney.Domain.Enums;

namespace YourMoney.Domain.Entities
{
    public class Receita : BaseEntity
    {
        public string Descricao { get; private set; }
        public Decimal Valor { get; private set; }
        public DateTime Data { get; private set; }
        public DateTime? MesReferencia { get; private set; }
        public NaturezaReceita Natureza { get; private set; } = NaturezaReceita.RendaDisponivel;
        public Guid? DespesaVinculadaId { get; private set; }
        public virtual Despesa DespesaVinculada { get; private set; }
        //public Guid CategoriaId { get; private set; }  
        // public virtual Categoria Categoria { get; private set; }  
        //public bool Recebida { get; private set; }
        //public DateTime? DataRecebimento { get; private set; }
        //public TipoRecorrencia TipoRecorrencia { get; private set; }  
        //public DateTime DataCriacao { get; private set; }

        private Receita() { }

        public Receita(string descricao, Decimal valor, DateTime data)
        {
            Id = Guid.NewGuid();
            AtualizarDescricao(descricao);
            AtualizarValor(valor);
            AtualizarData(data);
            // CategoriaId = categoriaId;  
            //TipoRecorrencia = tipoRecorrencia;  
            //Recebida = false;
            //DataCriacao = DateTime.Now;
        }

        public Receita(string descricao, Decimal valor, DateTime data, DateTime mesReferencia)
            : this(descricao, valor, data)
        {
            AtualizarMesReferencia(mesReferencia);
        }

        public Receita(string descricao, Decimal valor, DateTime data, DateTime mesReferencia, string usuarioId)
            : this(descricao, valor, data, mesReferencia)
        {
            DefinirUsuario(usuarioId);
        }

        public Receita(string descricao, Decimal valor, DateTime data, DateTime mesReferencia, string usuarioId, NaturezaReceita natureza, Guid? despesaVinculadaId = null)
            : this(descricao, valor, data, mesReferencia, usuarioId)
        {
            AtualizarNatureza(natureza, despesaVinculadaId);
        }

        public void AtualizarDescricao(string descricao)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                throw new ArgumentException("Descrição é obrigatória");
            Descricao = descricao.Trim();
        }

        public void AtualizarValor(Decimal valor)
        {
            if (valor <= 0)
                throw new ArgumentException("Valor deve ser maior que zero.", nameof(valor));
            Valor = valor;
        }

        public void AtualizarData(DateTime data)
        {
            if (data == default)
                throw new ArgumentException("Data é obrigatória.", nameof(data));

            Data = data.Date;
        }

        public void AtualizarMesReferencia(DateTime mesReferencia)
        {
            if (mesReferencia == default)
                throw new ArgumentException("Mês de referência é obrigatório.", nameof(mesReferencia));

            MesReferencia = new DateTime(mesReferencia.Year, mesReferencia.Month, 1);
        }

        public void AtualizarNatureza(NaturezaReceita natureza, Guid? despesaVinculadaId = null)
        {
            if (!Enum.IsDefined(typeof(NaturezaReceita), natureza))
                throw new ArgumentException("Natureza da receita é inválida.", nameof(natureza));

            Natureza = natureza;

            if (natureza == NaturezaReceita.Reembolso)
            {
                VincularDespesa(despesaVinculadaId);
                return;
            }

            LimparDespesaVinculada();
        }

        public void VincularDespesa(Guid? despesaId)
        {
            if (Natureza == NaturezaReceita.Reembolso && (!despesaId.HasValue || despesaId.Value == Guid.Empty))
                throw new ArgumentException("Despesa vinculada é obrigatória para reembolso.", nameof(despesaId));

            DespesaVinculadaId = despesaId;
        }

        public void LimparDespesaVinculada()
        {
            DespesaVinculadaId = null;
        }

        public bool ConsideraNasMetas => Natureza == NaturezaReceita.RendaDisponivel;

        //public void MarcarComoRecebida()
        //{
        //    Recebida = true;
        //    DataRecebimento = DateTime.Now;
        //}

        //public void DesmarcarRecebimento()
        //{
        //    Recebida = false;
        //    DataRecebimento = null;
        //}
    }
}
