using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Contracts;
using Portfolio.Api.Data;
using Portfolio.Api.Models;

namespace Portfolio.Api.Controllers;

[ApiController]
[Route("api/portfolio-users")]
public sealed class PortfolioUsersController(
    PortfolioDbContext database,
    IOutputCacheStore outputCacheStore) : ControllerBase
{
    [HttpGet]
    [OutputCache(PolicyName = "PortfolioUsersRead")]
    public async Task<ActionResult<IReadOnlyList<PortfolioUserDto>>> GetAll()
    {
        var users = await database.PortfolioUsers
            .Include(user => user.Skills)
            .OrderBy(user => user.FullName)
            .ToListAsync();

        return Ok(users.Select(ToDto).ToArray());
    }

    [HttpGet("{id:int}")]
    [OutputCache(PolicyName = "PortfolioUsersRead")]
    public async Task<ActionResult<PortfolioUserDto>> GetById(int id)
    {
        var user = await database.PortfolioUsers
            .Include(item => item.Skills)
            .FirstOrDefaultAsync(item => item.Id == id);

        return user is null ? NotFound() : Ok(ToDto(user));
    }

    [HttpPost]
    public async Task<ActionResult<PortfolioUserDto>> Create(SavePortfolioUserRequest request)
    {
        if (await database.PortfolioUsers.AnyAsync(user => user.Email == request.Email.Trim()))
        {
            return Conflict(new { error = "A portfolio already uses that email address." });
        }

        var user = new PortfolioUser
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            Bio = Clean(request.Bio),
            Skills = await ResolveSkills(request.SkillsCsv)
        };

        database.PortfolioUsers.Add(user);
        await database.SaveChangesAsync();
        await outputCacheStore.EvictByTagAsync("portfolio-users", HttpContext.RequestAborted);

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, ToDto(user));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PortfolioUserDto>> Update(int id, SavePortfolioUserRequest request)
    {
        var user = await database.PortfolioUsers
            .Include(item => item.Skills)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (user is null)
        {
            return NotFound();
        }

        if (await database.PortfolioUsers.AnyAsync(item => item.Id != id && item.Email == request.Email.Trim()))
        {
            return Conflict(new { error = "A portfolio already uses that email address." });
        }

        user.FullName = request.FullName.Trim();
        user.Email = request.Email.Trim();
        user.Bio = Clean(request.Bio);
        user.Skills = await ResolveSkills(request.SkillsCsv);

        await database.SaveChangesAsync();
        await outputCacheStore.EvictByTagAsync("portfolio-users", HttpContext.RequestAborted);
        await outputCacheStore.EvictByTagAsync("projects", HttpContext.RequestAborted);

        return Ok(ToDto(user));
    }

    private async Task<ICollection<Skill>> ResolveSkills(string? skillsCsv)
    {
        var names = (skillsCsv ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(name => name.Length <= 80)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(20);
        var skills = new List<Skill>();

        foreach (var name in names)
        {
            var skill = await database.Skills.FirstOrDefaultAsync(item => item.Name == name);
            if (skill is null)
            {
                skill = new Skill { Name = name };
                database.Skills.Add(skill);
            }

            skills.Add(skill);
        }

        return skills;
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static PortfolioUserDto ToDto(PortfolioUser user) => new(
        user.Id,
        user.FullName,
        user.Email,
        user.Bio,
        user.Skills.Select(skill => skill.Name).OrderBy(name => name).ToArray());
}