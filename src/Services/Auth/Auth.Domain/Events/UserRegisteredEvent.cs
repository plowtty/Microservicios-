namespace Auth.Domain.Events;

using Shared.Common;

public record UserRegisteredEvent(Guid UserId, string Email, string Role) : DomainEvent;
