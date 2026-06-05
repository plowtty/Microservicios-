namespace Products.Application.Commands.CreateProduct;

using MediatR;
using Shared.Common;

public record CreateProductCommand(
    string Name,
    string Description,
    string Category,
    decimal Price,
    string Currency,
    int InitialStock
) : IRequest<Result<Guid>>;
