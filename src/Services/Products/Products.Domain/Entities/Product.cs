namespace Products.Domain.Entities;

using Products.Domain.Enums;
using Products.Domain.Events;
using Products.Domain.Exceptions;
using Products.Domain.ValueObjects;
using Shared.Common;

public class Product : AggregateRoot
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Category { get; private set; }
    public Money Price { get; private set; }
    public int StockQuantity { get; private set; }
    public ProductStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Product()
    {
        Name = string.Empty;
        Description = string.Empty;
        Category = string.Empty;
        Price = Money.Zero();
    }

    public static Product Create(string name, string description, string category, Money price, int initialStock)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegative(initialStock);

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Category = category,
            Price = price,
            StockQuantity = initialStock,
            Status = initialStock > 0 ? ProductStatus.Active : ProductStatus.OutOfStock,
            CreatedAt = DateTime.UtcNow
        };

        product.RaiseDomainEvent(new ProductCreatedEvent(product.Id, name, price.Amount));
        return product;
    }

    public void ReduceStock(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        if (StockQuantity < quantity)
            throw new InsufficientStockException(Id, quantity, StockQuantity);

        var previous = StockQuantity;
        StockQuantity -= quantity;
        Status = StockQuantity == 0 ? ProductStatus.OutOfStock : ProductStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new StockUpdatedEvent(Id, previous, StockQuantity));
    }

    public void AddStock(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        var previous = StockQuantity;
        StockQuantity += quantity;
        if (Status == ProductStatus.OutOfStock)
            Status = ProductStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new StockUpdatedEvent(Id, previous, StockQuantity));
    }

    public void Deactivate()
    {
        Status = ProductStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(Money newPrice)
    {
        Price = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasSufficientStock(int quantity) => StockQuantity >= quantity;
}
