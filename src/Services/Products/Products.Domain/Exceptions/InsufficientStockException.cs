namespace Products.Domain.Exceptions;

public class InsufficientStockException : Exception
{
    public InsufficientStockException(Guid productId, int requested, int available)
        : base($"Product '{productId}' has insufficient stock. Requested: {requested}, Available: {available}.") { }
}
