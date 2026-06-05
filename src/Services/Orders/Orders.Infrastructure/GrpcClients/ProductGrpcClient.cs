namespace Orders.Infrastructure.GrpcClients;

using Orders.Application.Interfaces;
using Products.Grpc;

public class ProductGrpcClient : IProductService
{
    private readonly ProductService.ProductServiceClient _client;

    public ProductGrpcClient(ProductService.ProductServiceClient client) =>
        _client = client;

    public async Task<bool> CheckStockAsync(IEnumerable<Orders.Application.Interfaces.StockCheckItem> items, CancellationToken cancellationToken = default)
    {
        var request = new CheckStockRequest();
        request.Items.AddRange(items.Select(i => new global::Products.Grpc.StockCheckItem
        {
            ProductId = i.ProductId.ToString(),
            RequestedQuantity = i.Quantity
        }));

        var response = await _client.CheckStockAsync(request, cancellationToken: cancellationToken);
        return response.AllAvailable;
    }

    public async Task<ProductDetails?> GetProductDetailsAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetProductDetailsAsync(
            new GetProductDetailsRequest { ProductId = productId.ToString() },
            cancellationToken: cancellationToken);

        return response is null ? null : new ProductDetails(
            Guid.Parse(response.ProductId),
            response.Name,
            (decimal)response.UnitPrice,
            response.IsActive);
    }
}
