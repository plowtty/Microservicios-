namespace Orders.Domain.ValueObjects;

public record CustomerId(Guid Value)
{
    public static CustomerId New() => new(Guid.NewGuid());
    public static CustomerId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
