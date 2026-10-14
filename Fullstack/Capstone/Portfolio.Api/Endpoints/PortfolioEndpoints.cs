using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Contracts;
using Portfolio.Api.Data;
using Portfolio.Api.Models;

namespace Portfolio.Api.Endpoints;

public static class PortfolioEndpoints
{
    public static void MapPortfolioEndpoints(this IEndpointRouteBuilder app)
    {
        var users = app.MapGroup("/api/portfolio-users");
        users.MapGet("", GetUsers);
        users.MapGet("/{id:int}", GetUser);
        users.MapPost("", CreateUser);
        users.MapPut("/{id:int}", UpdateUser);

        var projects = app.MapGroup("/api/projects");
        projects.MapGet("", GetProjects);
        projects.MapPost("", CreateProject);
        projects.MapPut("/{id:int}", UpdateProject);
    }

    private static async Task<IResult> GetUsers(PortfolioDbContext database)
    {
        var users = await database.PortfolioUsers
            .Include(user => user.Skills)
            .OrderBy(user => user.FullName)
            .ToListAsync();

        return TypedResults.Ok(users.Select(ToDto));
    }

    private static async Task<IResult> GetUser(int id, PortfolioDbContext database)
    {
        var user = await database.PortfolioUsers
            .Include(item => item.Skills)
            .FirstOrDefaultAsync(item => item.Id == id);

        return user is null ? TypedResults.NotFound() : TypedResults.Ok(ToDto(user));
    }

    private static async Task<IResult> CreateUser(
        SavePortfolioUserRequest request,
        PortfolioDbContext database)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        if (await database.PortfolioUsers.AnyAsync(user => user.Email == request.Email.Trim()))
        {
            return TypedResults.Conflict(new { error = "A portfolio already uses that email address." });
        }

        var user = new PortfolioUser
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            Bio = Clean(request.Bio),
            Skills = await ResolveSkills(request.SkillsCsv, database)
        };

        database.PortfolioUsers.Add(user);
        await database.SaveChangesAsync();
        return TypedResults.Created($"/api/portfolio-users/{user.Id}", ToDto(user));
    }

    private static async Task<IResult> UpdateUser(
        int id,
        SavePortfolioUserRequest request,
        PortfolioDbContext database)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var user = await database.PortfolioUsers
            .Include(item => item.Skills)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (user is null)
        {
            return TypedResults.NotFound();
        }

        if (await database.PortfolioUsers.AnyAsync(item => item.Id != id && item.Email == request.Email.Trim()))
        {
            return TypedResults.Conflict(new { error = "A portfolio already uses that email address." });
        }

        user.FullName = request.FullName.Trim();
        user.Email = request.Email.Trim();
        user.Bio = Clean(request.Bio);
        user.Skills = await ResolveSkills(request.SkillsCsv, database);

        await database.SaveChangesAsync();
        return TypedResults.Ok(ToDto(user));
    }

    private static async Task<IResult> GetProjects(
        int? portfolioUserId,
        PortfolioDbContext database)
    {
        var query = database.Projects.Include(project => project.PortfolioUser).AsQueryable();
        if (portfolioUserId.HasValue)
        {
            query = query.Where(project => project.PortfolioUserId == portfolioUserId.Value);
        }

        var projects = await query.OrderByDescending(project => project.Id).ToListAsync();
        return TypedResults.Ok(projects.Select(ToDto));
    }

    private static async Task<IResult> CreateProject(
        int portfolioUserId,
        SaveProjectRequest request,
        PortfolioDbContext database)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var user = await database.PortfolioUsers.FindAsync(portfolioUserId);
        if (user is null)
        {
            return TypedResults.NotFound(new { error = "The portfolio user was not found." });
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
        return TypedResults.Created($"/api/projects/{project.Id}", ToDto(project));
    }

    private static async Task<IResult> UpdateProject(
        int id,
        SaveProjectRequest request,
        PortfolioDbContext database)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var project = await database.Projects
            .Include(item => item.PortfolioUser)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (project is null)
        {
            return TypedResults.NotFound();
        }

        project.Title = request.Title.Trim();
        project.Description = Clean(request.Description);
        project.ProjectUrl = Clean(request.ProjectUrl);
        project.Status = request.Status.Trim();

        await database.SaveChangesAsync();
        return TypedResults.Ok(ToDto(project));
    }

    private static Dictionary<string, string[]> Validate(object request)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true);

        return results
            .SelectMany(result => result.MemberNames.DefaultIfEmpty(string.Empty)
                .Select(member => (member, message: result.ErrorMessage ?? "The value is invalid.")))
            .GroupBy(item => item.member)
            .ToDictionary(group => group.Key, group => group.Select(item => item.message).ToArray());
    }

    private static async Task<ICollection<Skill>> ResolveSkills(string? skillsCsv, PortfolioDbContext database)
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

    private static ProjectDto ToDto(Project project) => new(
        project.Id,
        project.PortfolioUserId,
        project.PortfolioUser.FullName,
        project.Title,
        project.Description,
        project.ProjectUrl,
        project.Status);
}