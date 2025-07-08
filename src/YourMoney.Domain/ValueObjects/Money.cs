using System;

namespace YourMoney.Domain.ValueObjects

{
    public class Money : IEquatable<Money>
{
    public decimal Valor { get; private set; }
    public string Moeda { get; private set; }

    public Money(decimal valor, string moeda = "BRL")
    {
        if (valor < 0)
            throw new ArgumentException("Valor não pode ser negativo");

        if (string.IsNullOrWhiteSpace(moeda))
            throw new ArgumentException("Moeda deve ser informada");

        Valor = Math.Round(valor, 2);
        Moeda = moeda.ToUpper();
    }

    public Money Somar(Money other)
    {
        if (Moeda != other.Moeda)
            throw new InvalidOperationException("Não é possível somar valores de moedas diferentes");

        return new Money(Valor + other.Valor, Moeda);
    }

    public Money Subtrair(Money other)
    {
        if (Moeda != other.Moeda)
            throw new InvalidOperationException("Não é possível subtrair valores de moedas diferentes");

        return new Money(Valor - other.Valor, Moeda);
    }

    public bool Equals(Money other)
    {
        if (other is null) return false;
        return Valor == other.Valor && Moeda == other.Moeda;
    }

    public override bool Equals(object obj) => Equals(obj as Money);
    public override int GetHashCode() => HashCode.Combine(Valor, Moeda);

    public static bool operator ==(Money left, Money right) => left?.Equals(right) ?? right is null;
    public static bool operator !=(Money left, Money right) => !(left == right);
}
}