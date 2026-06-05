namespace Products.Application.Queries.GetProducts;

using MediatR;
using Products.Application.DTOs;
using Shared.Common;

public record GetProductsQuery(string? Category = null) : IRequest<Result<IEnumerable<ProductDto>>>;
