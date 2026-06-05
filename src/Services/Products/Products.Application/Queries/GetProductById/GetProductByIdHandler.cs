namespace Products.Application.Queries.GetProductById;

using AutoMapper;
using MediatR;
using Products.Application.DTOs;
using Products.Application.Interfaces;
using Shared.Common;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;

    public GetProductByIdHandler(IProductRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.ProductId, cancellationToken);
        return product is null
            ? Result<ProductDto>.Failure($"Product '{request.ProductId}' not found.")
            : Result<ProductDto>.Success(_mapper.Map<ProductDto>(product));
    }
}
