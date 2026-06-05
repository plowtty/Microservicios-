namespace Products.Application.DTOs;

public record CreateProductRequest(
    string Name,
    string Description,
    string Category,
    decimal Price,
    string Currency,
    int InitialStock
);
