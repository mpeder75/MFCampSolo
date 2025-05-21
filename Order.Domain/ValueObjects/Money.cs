using Order.Domain.Exceptions;

namespace Order.Domain.ValueObjects;

public record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new NegativeAmountException("Amount needs to be a positive");
        }

        if (string.IsNullOrEmpty(currency))
        {
            throw new InvalidCurrencyException("currency needs to be filled correctly");
        }

        return new Money(amount, currency);
    }

    public static Money Zero(string currency)
    {
        return Create(0, currency);
    }

    public Money Add(Money summand)
    {
        if (Currency != summand.Currency)
        {
            throw new CurrencyMismatchException("Cannot add Money with different currencies");
        }

        return new Money(Amount + summand.Amount, Currency);
    }

    public Money Subtract(Money subtrahend)
    {
        if (Currency != subtrahend.Currency)
        {
            throw new CurrencyMismatchException("Cannot subtract two different currencies");
        }

        return new Money(Amount - subtrahend.Amount, Currency);
    }

    public Money Multiply(decimal multiplier)
    {
        if (multiplier == 0)
        {
            throw new ZeroMultiplierException("Multiplier cannot be zero");
        }

        if (multiplier < 0)
        {
            throw new NegativeAmountException("Multiplier cannot be negative");
        }

        return new Money(Amount * multiplier, Currency);
    }

    public static Money operator +(Money summand1, Money summand2)
    {
        return summand1.Add(summand2);
    }

    public static Money operator -(Money minuend, Money subtrahend)
    {
        return minuend.Subtract(subtrahend);
    }

    public static Money operator *(Money multiplicand, decimal multiplier)
    {
        return multiplicand.Multiply(multiplier);
    }
}