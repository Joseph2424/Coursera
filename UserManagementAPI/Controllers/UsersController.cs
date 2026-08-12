using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Models;
using UserManagementAPI.Services;

namespace UserManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController(UserRepository repo) : ControllerBase
    {
        private readonly UserRepository _repo = repo;

        [HttpGet]
        public IActionResult GetAll() =>
            Ok(_repo.GetAll());

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var user = _repo.Get(id);
            return user == null
                ? NotFound(new { message = $"User {id} not found" })
                : Ok(user);
        }

        [HttpPost]
        public IActionResult Create(User user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = _repo.Create(user);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, User user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = _repo.Update(id, user);
            return success
                ? NoContent()
                : NotFound(new { message = $"User {id} not found" });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var success = _repo.Delete(id);
            return success
                ? NoContent()
                : NotFound(new { message = $"User {id} not found" });
        }

        [HttpGet("search")]
        public IActionResult Search([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest(new { message = "Search term cannot be empty" });

            var results = _repo.Search(term);
            return Ok(results);
        }
    }
}
