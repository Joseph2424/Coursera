using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Models;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CountersController(CounterDbContext db) : ControllerBase
{
    private readonly CounterDbContext _db = db;

    // GET: api/counters
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Counter>>> GetCounters()
    {
        return await _db.Counters.ToListAsync();
    }

    // POST: api/counters/{name}
    // Adds a new named counter with initial value 0
    [HttpPost("{name}")]
    public async Task<ActionResult<Counter>> AddCounter(string name)
    {
        if (await _db.Counters.AnyAsync(c => c.Name == name))
            return Conflict($"Counter '{name}' already exists.");

        var counter = new Counter { Name = name, Value = 0 };
        _db.Counters.Add(counter);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCounter), new { name = counter.Name }, counter);
    }

    // GET: api/counters/{name}
    [HttpGet("{name}")]
    public async Task<ActionResult<Counter>> GetCounter(string name)
    {
        var counter = await _db.Counters.FindAsync(name);
        if (counter == null)
            return NotFound();

        return counter;
    }

    // PUT: api/counters/{name}/increment
    [HttpPut("{name}/increment")]
    public async Task<ActionResult<Counter>> IncrementCounter(string name)
    {
        var counter = await _db.Counters.FindAsync(name);
        if (counter == null)
            return NotFound();

        counter.Value++;
        await _db.SaveChangesAsync();

        return counter;
    }

    // DELETE: api/counters/{name}
    [HttpDelete("{name}")]
    public async Task<IActionResult> DeleteCounter(string name)
    {
        var counter = await _db.Counters.FindAsync(name);
        if (counter == null)
            return NotFound();

        _db.Counters.Remove(counter);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
