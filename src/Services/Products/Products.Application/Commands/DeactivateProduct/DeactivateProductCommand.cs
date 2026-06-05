namespace Products.Application.Commands.DeactivateProduct;

using MediatR;
using Shared.Common;

public record DeactivateProductCommand(Guid ProductId) : IRequest<Result>;
