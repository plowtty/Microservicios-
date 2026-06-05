namespace Products.Domain.Events;

using Shared.Common;

public record ProductCreatedEvent(Guid ProductId, string Name, decimal Price) : DomainEvent;
