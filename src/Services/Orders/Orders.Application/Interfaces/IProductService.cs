namespace Orders.Application.Interfaces;

public interface IProductService
{
    Task<bool> CheckStockAsync(IEnumerable<StockCheckItem> items, CancellationToken cancellationToken = default);
    Task<ProductDetails?> GetProductDetailsAsync(Guid productId, CancellationToken cancellationToken = default);
}

public record StockCheckItem(Guid ProductId, int Quantity);
public record ProductDetails(Guid ProductId, string Name, decimal UnitPrice, bool IsActive);
