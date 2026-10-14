using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Contracts;
using Portfolio.Api.Data;
using Portfolio.Api.Models;

namespace Portfolio.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/portfolio-users")]
public sealed class PortfolioUsersController(
    PortfolioDbContext database,
    IOutputCacheStore outputCacheStore) : ControllerBase
{
    [HttpGet]
    [OutputCache(PolicyName = "PortfolioUsersRead")]
    public async Task<ActionResult<IReadOnlyList<PortfolioUserDto>>> GetAll()
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (ownerId is null)
        {
            return Unauthorized();
        }

        var users = await database.PortfolioUsers
            .Include(user => user.SkillLinks)
                .ThenInclude(link => link.Skill)
            .Where(user => user.OwnerId == ownerId)
            .OrderBy(user => user.FullName)
            .ToListAsync();

        return Ok(users.Select(ToDto).ToArray());
    }

    [HttpGet("{id:int}")]
    [OutputCache(PolicyName = "PortfolioUsersRead")]
    public async Task<ActionResult<PortfolioUserDto>> GetById(int id)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (ownerId is null)
        {
            return Unauthorized();
        }

        var user = await database.PortfolioUsers
            .Include(item => item.SkillLinks)
                .ThenInclude(link => link.Skill)
            .FirstOrDefaultAsync(item => item.Id == id && item.OwnerId == ownerId);

        return user is null ? NotFound() : Ok(ToDto(user));
    }

    [HttpPost]
    public async Task<ActionResult<PortfolioUserDto>> Create(SavePortfolioUserRequest request)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (ownerId is null)
        {
            return Unauthorized();
        }

        if (await database.PortfolioUsers.AnyAsync(user => user.Email == request.Email.Trim()))
        {
            return Conflict(new { error = "A portfolio already uses that email address." });
        }

        var skills = await ResolveSkills(request.SkillsCsv);
        var user = new PortfolioUser
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            Bio = Clean(request.Bio),
            OwnerId = ownerId
        };

        database.PortfolioUsers.Add(user);
        foreach (var skill in skills)
        {
            database.PortfolioUserSkills.Add(new PortfolioUserSkill { PortfolioUser = user, Skill = skill });
        }

        await database.SaveChangesAsync();
        await outputCacheStore.EvictByTagAsync("portfolio-users", HttpContext.RequestAborted);

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, ToDto(user));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PortfolioUserDto>> Update(int id, SavePortfolioUserRequest request)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (ownerId is null)
        {
            return Unauthorized();
        }

        var user = await database.PortfolioUsers
            .Include(item => item.SkillLinks)
                .ThenInclude(link => link.Skill)
            .FirstOrDefaultAsync(item => item.Id == id && item.OwnerId == ownerId);
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
        var skills = await ResolveSkills(request.SkillsCsv);
        var skillNames = skills.Select(skill => skill.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var linksToRemove = user.SkillLinks
            .Where(link => !skillNames.Contains(link.Skill.Name))
            .ToList();
        database.PortfolioUserSkills.RemoveRange(linksToRemove);

        var existingSkillNames = user.SkillLinks
            .Where(link => !linksToRemove.Contains(link))
            .Select(link => link.Skill.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var skill in skills.Where(skill => !existingSkillNames.Contains(skill.Name)))
        {
            database.PortfolioUserSkills.Add(new PortfolioUserSkill { PortfolioUser = user, Skill = skill });
        }

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
        user.SkillLinks.Select(link => link.Skill.Name).OrderBy(name => name).ToArray());
}