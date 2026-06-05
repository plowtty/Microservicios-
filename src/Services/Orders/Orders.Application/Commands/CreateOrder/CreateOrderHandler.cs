namespace Orders.Application.Commands.CreateOrder;

using MediatR;
using Orders.Application.Interfaces;
using Orders.Domain.Entities;
using Orders.Domain.ValueObjects;
using Shared.Common;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductService _productService;

    public CreateOrderHandler(IOrderRepository orderRepository, IProductService productService)
    {
        _orderRepository = orderRepository;
        _productService = productService;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var stockItems = request.Items.Select(i => new StockCheckItem(i.ProductId, i.Quantity));
        var stockAvailable = await _productService.CheckStockAsync(stockItems, cancellationToken);

        if (!stockAvailable)
            return Result<Guid>.Failure("One or more products are out of stock.");

        var address = new Address(
            request.ShippingAddress.Street,
            request.ShippingAddress.City,
            request.ShippingAddress.State,
            request.ShippingAddress.Country,
            request.ShippingAddress.ZipCode);

        var order = Order.Create(CustomerId.From(request.CustomerId), address);

        foreach (var itemRequest in request.Items)
        {
            var product = await _productService.GetProductDetailsAsync(itemRequest.ProductId, cancellationToken);
            if (product is null)
                return Result<Guid>.Failure($"Product '{itemRequest.ProductId}' not found.");

            var item = OrderItem.Create(
                product.ProductId,
                product.Name,
                itemRequest.Quantity,
                new Money(product.UnitPrice, "USD"));

            order.AddItem(item);
        }

        await _orderRepository.AddAsync(order, cancellationToken);
        return Result<Guid>.Success(order.Id);
    }
}
