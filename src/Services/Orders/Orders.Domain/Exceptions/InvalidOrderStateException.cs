namespace Orders.Domain.Exceptions;

using Orders.Domain.Enums;

public class InvalidOrderStateException : Exception
{
    public InvalidOrderStateException(Guid orderId, OrderStatus currentStatus, string operation)
        : base($"Cannot {operation} order '{orderId}' in status '{currentStatus}'.") { }
}
