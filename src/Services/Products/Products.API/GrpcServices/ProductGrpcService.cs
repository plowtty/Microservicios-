namespace Products.API.GrpcServices;

using global::Grpc.Core;
using Products.Application.Interfaces;
using Products.Grpc;

public class ProductGrpcService : ProductService.ProductServiceBase
{
    private readonly IProductRepository _repository;

    public ProductGrpcService(IProductRepository repository) => _repository = repository;

    public override async Task<CheckStockResponse> CheckStock(CheckStockRequest request, ServerCallContext context)
    {
        var response = new CheckStockResponse { AllAvailable = true };

        foreach (var item in request.Items)
        {
            var productId = Guid.Parse(item.ProductId);
            var product = await _repository.GetByIdAsync(productId, context.CancellationToken);

            var isAvailable = product is not null && product.HasSufficientStock(item.RequestedQuantity);
            response.Results.Add(new StockCheckResult
            {
                ProductId = item.ProductId,
                IsAvailable = isAvailable,
                AvailableQuantity = product?.StockQuantity ?? 0
            });

            if (!isAvailable) response.AllAvailable = false;
        }

        return response;
    }

    public override async Task<GetProductDetailsResponse> GetProductDetails(GetProductDetailsRequest request, ServerCallContext context)
    {
        var productId = Guid.Parse(request.ProductId);
        var product = await _repository.GetByIdAsync(productId, context.CancellationToken);

        if (product is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Product '{request.ProductId}' not found."));

        return new GetProductDetailsResponse
        {
            ProductId = product.Id.ToString(),
            Name = product.Name,
            UnitPrice = (double)product.Price.Amount,
            IsActive = product.Status == Domain.Enums.ProductStatus.Active
        };
    }
}
