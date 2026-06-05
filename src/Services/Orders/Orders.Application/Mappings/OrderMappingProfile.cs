namespace Orders.Application.Mappings;

using AutoMapper;
using Orders.Application.DTOs;
using Orders.Domain.Entities;
using Orders.Domain.ValueObjects;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<Order, OrderDto>()
            .ForMember(d => d.CustomerId, o => o.MapFrom(s => s.CustomerId.Value))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.TotalAmount, o => o.MapFrom(s => s.TotalAmount.Amount))
            .ForMember(d => d.Currency, o => o.MapFrom(s => s.TotalAmount.Currency));

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(d => d.UnitPrice, o => o.MapFrom(s => s.UnitPrice.Amount))
            .ForMember(d => d.TotalPrice, o => o.MapFrom(s => s.TotalPrice.Amount))
            .ForMember(d => d.Currency, o => o.MapFrom(s => s.UnitPrice.Currency));

        CreateMap<Address, AddressDto>();
        CreateMap<AddressDto, Address>();
    }
}
