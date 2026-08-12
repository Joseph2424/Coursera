using Microsoft.AspNetCore.Mvc;

namespace SampleWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private static readonly List<Product> Products = new()
        {
            new Product { Id = 1, Name = "Widget", Price = 9.99m },
            new Product { Id = 2, Name = "Gadget", Price = 19.99m }
        };

        [HttpGet]
        public ActionResult<IEnumerable<Product>> GetAll()
        {
            return Ok(Products);
        }

        [HttpGet("{id}")]
        public ActionResult<Product> Get(int id)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpPost]
        public ActionResult<Product> Create(Product product)
        {
            product.Id = Products.Any() ? Products.Max(p => p.Id) + 1 : 1;
            Products.Add(product);
            return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, Product updatedProduct)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            product.Name = updatedProduct.Name;
            product.Price = updatedProduct.Price;
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            Products.Remove(product);
            return NoContent();
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
