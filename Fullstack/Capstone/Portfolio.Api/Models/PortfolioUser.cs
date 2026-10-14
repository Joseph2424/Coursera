using System.ComponentModel.DataAnnotations;

namespace Portfolio.Api.Models;

public sealed class PortfolioUser
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(254)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(1200)]
    public string? Bio { get; set; }

    public string? OwnerId { get; set; }

    public ApplicationUser? Owner { get; set; }

    public ICollection<Project> Projects { get; set; } = new List<Project>();

    public ICollection<PortfolioUserSkill> SkillLinks { get; set; } = new List<PortfolioUserSkill>();
}