namespace Products.Application.Commands.CreateProduct;

using MediatR;
using Products.Application.Interfaces;
using Products.Domain.Entities;
using Products.Domain.ValueObjects;
using Shared.Common;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IProductRepository _repository;

    public CreateProductHandler(IProductRepository repository) => _repository = repository;

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = Product.Create(
            request.Name,
            request.Description,
            request.Category,
            new Money(request.Price, request.Currency),
            request.InitialStock);

        await _repository.AddAsync(product, cancellationToken);
        return Result<Guid>.Success(product.Id);
    }
}
