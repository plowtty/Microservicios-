namespace Orders.Application.DTOs;

public record OrderDto(
    Guid Id,
    Guid CustomerId,
    string Status,
    AddressDto ShippingAddress,
    List<OrderItemDto> Items,
    decimal TotalAmount,
    string Currency,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record AddressDto(
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode
);
