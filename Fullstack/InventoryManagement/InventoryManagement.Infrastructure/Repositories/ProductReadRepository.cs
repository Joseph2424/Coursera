using InventoryManagement.Domain.Contracts;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories;

public class ProductReadRepository(AppDbContext context) : IProductReadRepository
{
    private readonly AppDbContext _context = context;

    public Task<List<Product>> GetProductsAsync()
    {
        return _context
            .Products.AsNoTracking()
            .OrderByDescending(product => product.CreatedOn)
            .Select(product => new Product
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Quantity = product.Quantity,
                CreatedOn = product.CreatedOn,
                CategoryId = product.CategoryId,
            })
            .ToListAsync();
    }

    public Task<Product?> GetProductDetailsByIdAsync(Guid id)
    {
        return _context
            .Products.AsNoTracking()
            .Include(product => product.Category)
            .FirstOrDefaultAsync(product => product.Id == id);
    }
}
