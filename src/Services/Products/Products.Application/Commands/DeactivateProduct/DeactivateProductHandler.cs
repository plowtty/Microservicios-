namespace Products.Application.Commands.DeactivateProduct;

using MediatR;
using Products.Application.Interfaces;
using Shared.Common;

public class DeactivateProductHandler : IRequestHandler<DeactivateProductCommand, Result>
{
    private readonly IProductRepository _repository;

    public DeactivateProductHandler(IProductRepository repository) => _repository = repository;

    public async Task<Result> Handle(DeactivateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result.Failure($"Product '{request.ProductId}' not found.");

        product.Deactivate();
        await _repository.UpdateAsync(product, cancellationToken);
        return Result.Success();
    }
}
