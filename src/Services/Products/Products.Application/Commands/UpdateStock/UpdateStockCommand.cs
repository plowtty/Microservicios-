namespace Products.Application.Commands.UpdateStock;

using MediatR;
using Shared.Common;

public record UpdateStockCommand(Guid ProductId, int Quantity, StockOperation Operation) : IRequest<Result>;

public enum StockOperation { Add, Reduce }
