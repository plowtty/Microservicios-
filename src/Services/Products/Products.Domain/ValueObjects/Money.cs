namespace Products.Domain.ValueObjects;

public record Money(decimal Amount, string Currency = "USD")
{
    public static Money Zero() => new(0);
    public override string ToString() => $"{Amount:F2} {Currency}";
}
