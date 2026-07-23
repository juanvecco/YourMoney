namespace YourMoney.Domain.Entities
{
    public class Investimento : BaseEntity
    {
        public string Nome { get; private set; }
        public string Descricao { get; private set; }
        public string Tipo { get; private set; }
        public decimal Quantidade { get; private set; }
        public decimal PrecoMedio { get; private set; }
        public decimal ValorAtual { get; private set; }
        public DateTime DataInvestimento { get; private set; }
        public DateTime? MesReferencia { get; private set; }
        public DateTime? DataResgate { get; private set; }
        public bool Ativo { get; private set; }
        public Guid? ReceitaRecorrenteId { get; private set; }
        public virtual ReceitaRecorrente? ReceitaRecorrente { get; private set; }
        public Guid? OperacaoId { get; private set; }

        private Investimento() { }

        public Investimento(
            string nome,
            string descricao,
            string tipo,
            decimal quantidade,
            decimal precoMedio,
            decimal valorAtual,
            DateTime dataInvestimento,
            DateTime mesReferencia,
            Guid? receitaRecorrenteId = null,
            Guid? operacaoId = null)
        {
            Id = Guid.NewGuid();
            AtualizarNome(nome);
            AtualizarDescricao(descricao);
            AtualizarTipo(tipo);
            AtualizarQuantidade(quantidade);
            AtualizarPrecoMedio(precoMedio);
            AtualizarValorAtual(valorAtual);
            AtualizarData(dataInvestimento);
            AtualizarMesReferencia(mesReferencia);
            DefinirReceitaRecorrente(receitaRecorrenteId);
            DefinirOperacao(operacaoId);
            Ativo = true;
            DataResgate = null;
        }

        public Investimento(
            string nome,
            string descricao,
            string tipo,
            decimal quantidade,
            decimal precoMedio,
            decimal valorAtual,
            DateTime dataInvestimento,
            DateTime mesReferencia,
            string usuarioId,
            Guid? receitaRecorrenteId = null,
            Guid? operacaoId = null)
            : this(nome, descricao, tipo, quantidade, precoMedio, valorAtual, dataInvestimento, mesReferencia, receitaRecorrenteId, operacaoId)
        {
            DefinirUsuario(usuarioId);
        }

        public void AtualizarNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome do investimento é obrigatório.");
            if (nome.Trim().Length > 100)
                throw new ArgumentException("Nome do investimento deve ter no máximo 100 caracteres.");
            Nome = nome.Trim();
        }

        public void AtualizarDescricao(string descricao)
        {
            var descricaoNormalizada = descricao?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(descricaoNormalizada))
                throw new ArgumentException("Descrição do investimento é obrigatória.");
            if (descricaoNormalizada.Length > 500)
                throw new ArgumentException("Descrição deve ter no máximo 500 caracteres.");
            Descricao = descricaoNormalizada;
        }

        public void AtualizarTipo(string tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo))
                throw new ArgumentException("Tipo do investimento é obrigatório.");
            if (tipo.Trim().Length > 100)
                throw new ArgumentException("Tipo do investimento deve ter no máximo 100 caracteres.");
            Tipo = tipo.Trim();
        }

        public void AtualizarQuantidade(decimal quantidade)
        {
            if (quantidade <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero.");
            Quantidade = quantidade;
        }

        public void AtualizarPrecoMedio(decimal precoMedio)
        {
            if (precoMedio <= 0)
                throw new ArgumentException("Preço médio deve ser maior que zero.");
            PrecoMedio = precoMedio;
        }

        public void AtualizarValorAtual(decimal novoValor)
        {
            if (novoValor <= 0)
                throw new ArgumentException("Valor atual deve ser maior que zero.");
            ValorAtual = novoValor;
        }

        public void AtualizarData(DateTime data)
        {
            if (data == default)
                throw new ArgumentException("Data do investimento é obrigatória.");
            DataInvestimento = data.Date;
        }

        public void AtualizarMesReferencia(DateTime mesReferencia)
        {
            if (mesReferencia == default)
                throw new ArgumentException("Mês de referência é obrigatório.");

            MesReferencia = new DateTime(mesReferencia.Year, mesReferencia.Month, 1);
        }

        public void DefinirReceitaRecorrente(Guid? receitaRecorrenteId)
        {
            if (receitaRecorrenteId == Guid.Empty)
                throw new ArgumentException("Receita recorrente associada inválida.");

            ReceitaRecorrenteId = receitaRecorrenteId;
        }

        private void DefinirOperacao(Guid? operacaoId)
        {
            if (operacaoId == Guid.Empty)
                throw new ArgumentException("Identificador da operação inválido.");

            OperacaoId = operacaoId;
        }

        public void Resgatar()
        {
            Ativo = false;
            DataResgate = DateTime.Now;
        }
    }
}
