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
[Route("api/projects")]
public sealed class ProjectsController(
    PortfolioDbContext database,
    IOutputCacheStore outputCacheStore) : ControllerBase
{
    [HttpGet]
    [OutputCache(PolicyName = "ProjectsRead")]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> GetAll([FromQuery] int? portfolioUserId)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (ownerId is null)
        {
            return Unauthorized();
        }

        var query = database.Projects
            .AsNoTracking()
            .Where(project => project.PortfolioUser.OwnerId == ownerId);
        if (portfolioUserId.HasValue)
        {
            query = query.Where(project => project.PortfolioUserId == portfolioUserId.Value);
        }

        var projects = await query
            .OrderByDescending(project => project.Id)
            .Select(project => new ProjectDto(
                project.Id,
                project.PortfolioUserId,
                project.PortfolioUser.FullName,
                project.Title,
                project.Description,
                project.ProjectUrl,
                project.Status))
            .ToListAsync();
        return Ok(projects);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> Create(
        [FromQuery] int portfolioUserId,
        SaveProjectRequest request)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (ownerId is null)
        {
            return Unauthorized();
        }

        var user = await database.PortfolioUsers
            .FirstOrDefaultAsync(item => item.Id == portfolioUserId && item.OwnerId == ownerId);
        if (user is null)
        {
            return NotFound(new { error = "The portfolio user was not found." });
        }

        var project = new Project
        {
            PortfolioUserId = user.Id,
            PortfolioUser = user,
            Title = request.Title.Trim(),
            Description = Clean(request.Description),
            ProjectUrl = Clean(request.ProjectUrl),
            Status = request.Status.Trim()
        };

        database.Projects.Add(project);
        await database.SaveChangesAsync();
        await outputCacheStore.EvictByTagAsync("projects", HttpContext.RequestAborted);

        return Created($"/api/projects/{project.Id}", ToDto(project));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProjectDto>> Update(int id, SaveProjectRequest request)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (ownerId is null)
        {
            return Unauthorized();
        }

        var project = await database.Projects
            .Include(item => item.PortfolioUser)
            .FirstOrDefaultAsync(item => item.Id == id && item.PortfolioUser.OwnerId == ownerId);
        if (project is null)
        {
            return NotFound();
        }

        project.Title = request.Title.Trim();
        project.Description = Clean(request.Description);
        project.ProjectUrl = Clean(request.ProjectUrl);
        project.Status = request.Status.Trim();

        await database.SaveChangesAsync();
        await outputCacheStore.EvictByTagAsync("projects", HttpContext.RequestAborted);

        return Ok(ToDto(project));
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static ProjectDto ToDto(Project project) => new(
        project.Id,
        project.PortfolioUserId,
        project.PortfolioUser.FullName,
        project.Title,
        project.Description,
        project.ProjectUrl,
        project.Status);
}