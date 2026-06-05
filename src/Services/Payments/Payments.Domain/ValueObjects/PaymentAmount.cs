namespace Payments.Domain.ValueObjects;

public record PaymentAmount
{
    public decimal Value { get; }
    public string Currency { get; }

    public PaymentAmount(decimal value, string currency)
    {
        if (value <= 0) throw new ArgumentException("Amount must be positive", nameof(value));
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency is required", nameof(currency));
        Value = value;
        Currency = currency.ToUpperInvariant();
    }
}
