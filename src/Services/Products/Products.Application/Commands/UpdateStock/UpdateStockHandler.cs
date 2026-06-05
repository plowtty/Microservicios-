namespace Products.Application.Commands.UpdateStock;

using MediatR;
using Products.Application.Interfaces;
using Products.Domain.Exceptions;
using Shared.Common;

public class UpdateStockHandler : IRequestHandler<UpdateStockCommand, Result>
{
    private readonly IProductRepository _repository;

    public UpdateStockHandler(IProductRepository repository) => _repository = repository;

    public async Task<Result> Handle(UpdateStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result.Failure($"Product '{request.ProductId}' not found.");

        try
        {
            if (request.Operation == StockOperation.Add)
                product.AddStock(request.Quantity);
            else
                product.ReduceStock(request.Quantity);
        }
        catch (InsufficientStockException ex)
        {
            return Result.Failure(ex.Message);
        }

        await _repository.UpdateAsync(product, cancellationToken);
        return Result.Success();
    }
}
