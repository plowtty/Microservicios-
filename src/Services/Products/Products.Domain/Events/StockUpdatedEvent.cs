namespace Products.Domain.Events;

using Shared.Common;

public record StockUpdatedEvent(Guid ProductId, int PreviousStock, int NewStock) : DomainEvent;
