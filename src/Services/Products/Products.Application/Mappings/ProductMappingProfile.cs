namespace Products.Application.Mappings;

using AutoMapper;
using Products.Application.DTOs;
using Products.Domain.Entities;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.Price, o => o.MapFrom(s => s.Price.Amount))
            .ForMember(d => d.Currency, o => o.MapFrom(s => s.Price.Currency))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
    }
}
