namespace Orders.Domain.Exceptions;

public class OrderNotFoundException : Exception
{
    public OrderNotFoundException(Guid orderId)
        : base($"Order with ID '{orderId}' was not found.") { }
}
