namespace Orders.Domain.ValueObjects;

public record Address(
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode
)
{
    public override string ToString() => $"{Street}, {City}, {State} {ZipCode}, {Country}";
}
