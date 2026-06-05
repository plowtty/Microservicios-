namespace Orders.Application.DTOs;

public record CreateOrderRequest(
    Guid CustomerId,
    AddressDto ShippingAddress,
    List<CreateOrderItemRequest> Items
);

public record CreateOrderItemRequest(Guid ProductId, int Quantity);
