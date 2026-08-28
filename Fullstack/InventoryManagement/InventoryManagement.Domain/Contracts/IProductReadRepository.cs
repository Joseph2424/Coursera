using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Domain.Contracts;

public interface IProductReadRepository
{
    Task<List<Product>> GetProductsAsync();

    Task<Product?> GetProductDetailsByIdAsync(Guid id);
}
