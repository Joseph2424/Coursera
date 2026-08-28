using AutoMapper;
using InventoryManagement.Application.DTOs.Product;
using InventoryManagement.Domain.Contracts;

namespace InventoryManagement.Application.Services;

public class ProductService(IProductReadRepository productReadRepository, IMapper mapper) : IProductService
{
    private readonly IProductReadRepository _productReadRepository = productReadRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<GetProduct?> GetByIdAsync(Guid id)
    {
        var result = await _productReadRepository.GetProductDetailsByIdAsync(id);
        return result != null ? _mapper.Map<GetProduct>(result) : null;
    }
}
