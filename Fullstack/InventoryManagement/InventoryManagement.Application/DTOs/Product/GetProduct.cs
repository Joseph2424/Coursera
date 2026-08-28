using System.ComponentModel.DataAnnotations;
using InventoryManagement.Application.DTOs.Category;

namespace InventoryManagement.Application.DTOs.Product;

public class GetProduct : ProductBase
{
    [Required]
    public Guid Id { get; set; }

    public GetCategory? Category { get; set; }

    public DateTime CreatedOn { get; set; }
}
