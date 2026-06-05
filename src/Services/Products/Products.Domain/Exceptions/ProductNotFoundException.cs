namespace Products.Domain.Exceptions;

public class ProductNotFoundException : Exception
{
    public ProductNotFoundException(Guid id)
        : base($"Product '{id}' was not found.") { }
}
