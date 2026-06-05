namespace Products.Application.Queries.GetProducts;

using AutoMapper;
using MediatR;
using Products.Application.DTOs;
using Products.Application.Interfaces;
using Shared.Common;

public class GetProductsHandler : IRequestHandler<GetProductsQuery, Result<IEnumerable<ProductDto>>>
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;

    public GetProductsHandler(IProductRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = string.IsNullOrWhiteSpace(request.Category)
            ? await _repository.GetAllAsync(cancellationToken)
            : await _repository.GetByCategoryAsync(request.Category, cancellationToken);

        return Result<IEnumerable<ProductDto>>.Success(_mapper.Map<IEnumerable<ProductDto>>(products));
    }
}
