using InventoryManagement.Application.DTOs.Product;
using InventoryManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController(IProductService productService) : ControllerBase
{
    private readonly IProductService _productService = productService;

    [HttpGet("single/{id}")]
    public async Task<ActionResult<GetProduct>> GetSingle(Guid id)
    {
        var data = await _productService.GetByIdAsync(id);
        return data != null ? Ok(data) : NotFound();
    }
}
