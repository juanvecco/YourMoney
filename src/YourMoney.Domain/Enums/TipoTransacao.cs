namespace YourMoney.Domain.Enums
{
    public enum TipoTransacao
    {
        Receita = 1,
        Despesa = 2,
        Investimento = 3,
        Transferencia = 4
    }

    public enum StatusMeta
    {
        Ativa = 1,
        Pausada = 2,
        Concluida = 3,
        Cancelada = 4
    }

    public enum TipoRecorrencia
    {
        Unica = 0,
        Semanal = 1,
        Quinzenal = 2,
        Mensal = 3,
        Bimestral = 4,
        Trimestral = 5,
        Semestral = 6,
        Anual = 7
    }
}