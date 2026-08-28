using InventoryManagement.Application.DTOs.Product;

namespace InventoryManagement.Application.Services;

public interface IProductService
{
    Task<GetProduct?> GetByIdAsync(Guid id);
}
