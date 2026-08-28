using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Application.DTOs.Category;

public class CategoryBase
{
    [Required]
    public string? Name { get; set; }
}
