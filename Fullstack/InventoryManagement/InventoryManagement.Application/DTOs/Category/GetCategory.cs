using InventoryManagement.Application.DTOs.Product;

namespace InventoryManagement.Application.DTOs.Category;

public class GetCategory : CategoryBase
{
    public Guid Id { get; set; }

    public ICollection<GetProduct>? Products { get; set; }
}
