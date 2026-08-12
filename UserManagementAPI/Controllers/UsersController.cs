using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Models;
using UserManagementAPI.Services;

namespace UserManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserRepository _repo;

        public UsersController(UserRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public IActionResult GetAll() =>
            Ok(_repo.GetAll());

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var user = _repo.Get(id);
            return user == null ? NotFound() : Ok(user);
        }

        [HttpPost]
        public IActionResult Create(User user)
        {
            var created = _repo.Create(user);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, User user)
        {
            var success = _repo.Update(id, user);
            return success ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var success = _repo.Delete(id);
            return success ? NoContent() : NotFound();
        }

        [HttpGet("search")]
        public IActionResult Search([FromQuery] string term)
        {
            var results = _repo.Search(term);
            return Ok(results);
        }
    }
}
