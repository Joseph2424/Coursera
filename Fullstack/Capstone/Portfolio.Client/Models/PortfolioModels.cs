using System.ComponentModel.DataAnnotations;

namespace Portfolio.Client.Models;

public sealed record PortfolioUserDto(
    int Id,
    string FullName,
    string Email,
    string? Bio,
    IReadOnlyList<string> Skills);

public sealed record ProjectDto(
    int Id,
    int PortfolioUserId,
    string PortfolioUserName,
    string Title,
    string? Description,
    string? ProjectUrl,
    string Status);

public sealed record AuthenticatedUserDto(string Id, string Email);

public sealed record AuthResponseDto(string AccessToken, DateTimeOffset ExpiresAt, AuthenticatedUserDto User);

public sealed class AuthForm
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 10)]
    public string Password { get; set; } = string.Empty;
}

public sealed class PortfolioUserForm
{
    public int? Id { get; set; }

    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [StringLength(1200)]
    public string? Bio { get; set; }

    [StringLength(1000)]
    public string? SkillsCsv { get; set; }
}

public sealed class ProjectForm
{
    [Required, StringLength(140)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2400)]
    public string? Description { get; set; }

    [Url, StringLength(500)]
    public string? ProjectUrl { get; set; }

    [Required, StringLength(40)]
    public string Status { get; set; } = "In progress";
}