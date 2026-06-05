namespace Orders.UnitTests.Domain;

using FluentAssertions;
using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Orders.Domain.Exceptions;
using Orders.Domain.ValueObjects;
using Xunit;

public class OrderTests
{
    private static Order CreateOrder() => Order.Create(
        CustomerId.New(),
        new Address("123 Main St", "New York", "NY", "USA", "10001"));

    private static OrderItem CreateItem(decimal price = 10m) =>
        OrderItem.Create(Guid.NewGuid(), "Test Product", 2, new Money(price, "USD"));

    [Fact]
    public void Create_ShouldReturnPendingOrder_WithDomainEvent()
    {
        var order = CreateOrder();
        order.Status.Should().Be(OrderStatus.Pending);
        order.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void AddItem_WhenPending_ShouldAddItem()
    {
        var order = CreateOrder();
        order.AddItem(CreateItem());
        order.Items.Should().HaveCount(1);
    }

    [Fact]
    public void AddItem_WhenCancelled_ShouldThrow()
    {
        var order = CreateOrder();
        order.AddItem(CreateItem());
        order.Cancel("test");
        var act = () => order.AddItem(CreateItem());
        act.Should().Throw<InvalidOrderStateException>();
    }

    [Fact]
    public void Confirm_WithItems_ShouldConfirm()
    {
        var order = CreateOrder();
        order.AddItem(CreateItem());
        order.Confirm();
        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void Confirm_WithNoItems_ShouldThrow()
    {
        var order = CreateOrder();
        var act = () => order.Confirm();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void TotalAmount_ShouldSumAllItems()
    {
        var order = CreateOrder();
        order.AddItem(CreateItem(10m));
        order.AddItem(CreateItem(20m));
        order.TotalAmount.Amount.Should().Be(60m);
    }

    [Fact]
    public void Cancel_WhenConfirmed_ShouldCancel()
    {
        var order = CreateOrder();
        order.AddItem(CreateItem());
        order.Confirm();
        order.Cancel("changed mind");
        order.Status.Should().Be(OrderStatus.Cancelled);
    }
}
