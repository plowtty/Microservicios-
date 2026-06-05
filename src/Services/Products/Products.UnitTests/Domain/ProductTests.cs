namespace Products.UnitTests.Domain;

using FluentAssertions;
using Products.Domain.Entities;
using Products.Domain.Enums;
using Products.Domain.Exceptions;
using Products.Domain.ValueObjects;
using Xunit;

public class ProductTests
{
    private static Product CreateProduct(int stock = 10) =>
        Product.Create("Test Product", "A description", "Electronics", new Money(99.99m, "USD"), stock);

    [Fact]
    public void Create_ShouldSetActiveStatus_WhenStockIsPositive()
    {
        var product = CreateProduct(stock: 5);
        product.Status.Should().Be(ProductStatus.Active);
        product.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Create_ShouldSetOutOfStock_WhenInitialStockIsZero()
    {
        var product = CreateProduct(stock: 0);
        product.Status.Should().Be(ProductStatus.OutOfStock);
    }

    [Fact]
    public void Create_ShouldThrow_WhenStockIsNegative()
    {
        var act = () => CreateProduct(stock: -1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ReduceStock_ShouldDecreaseQuantity()
    {
        var product = CreateProduct(stock: 10);
        product.ReduceStock(3);
        product.StockQuantity.Should().Be(7);
    }

    [Fact]
    public void ReduceStock_WhenExactAmount_ShouldSetOutOfStock()
    {
        var product = CreateProduct(stock: 5);
        product.ReduceStock(5);
        product.Status.Should().Be(ProductStatus.OutOfStock);
        product.StockQuantity.Should().Be(0);
    }

    [Fact]
    public void ReduceStock_WhenInsufficientStock_ShouldThrow()
    {
        var product = CreateProduct(stock: 3);
        var act = () => product.ReduceStock(10);
        act.Should().Throw<InsufficientStockException>();
    }

    [Fact]
    public void ReduceStock_ShouldRaiseStockUpdatedEvent()
    {
        var product = CreateProduct(stock: 10);
        product.ClearDomainEvents();
        product.ReduceStock(2);
        product.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void AddStock_ShouldIncreaseQuantity()
    {
        var product = CreateProduct(stock: 5);
        product.AddStock(10);
        product.StockQuantity.Should().Be(15);
    }

    [Fact]
    public void AddStock_WhenOutOfStock_ShouldRestoreActiveStatus()
    {
        var product = CreateProduct(stock: 0);
        product.AddStock(5);
        product.Status.Should().Be(ProductStatus.Active);
    }

    [Fact]
    public void AddStock_WhenZero_ShouldThrow()
    {
        var product = CreateProduct();
        var act = () => product.AddStock(0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Deactivate_ShouldSetInactiveStatus()
    {
        var product = CreateProduct();
        product.Deactivate();
        product.Status.Should().Be(ProductStatus.Inactive);
    }

    [Fact]
    public void HasSufficientStock_ShouldReturnTrue_WhenStockCoversRequest()
    {
        var product = CreateProduct(stock: 10);
        product.HasSufficientStock(10).Should().BeTrue();
        product.HasSufficientStock(11).Should().BeFalse();
    }

    [Fact]
    public void UpdatePrice_ShouldChangePrice()
    {
        var product = CreateProduct();
        var newPrice = new Money(199.99m, "USD");
        product.UpdatePrice(newPrice);
        product.Price.Should().Be(newPrice);
        product.UpdatedAt.Should().NotBeNull();
    }
}
