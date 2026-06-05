namespace Products.Application.Queries.GetProductById;

using MediatR;
using Products.Application.DTOs;
using Shared.Common;

public record GetProductByIdQuery(Guid ProductId) : IRequest<Result<ProductDto>>;
