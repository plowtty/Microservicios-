namespace Products.Application.DTOs;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    string Category,
    decimal Price,
    string Currency,
    int StockQuantity,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
