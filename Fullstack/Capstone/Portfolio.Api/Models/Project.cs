using System.ComponentModel.DataAnnotations;

namespace Portfolio.Api.Models;

public sealed class Project
{
    public int Id { get; set; }

    public int PortfolioUserId { get; set; }

    public PortfolioUser PortfolioUser { get; set; } = null!;

    [Required]
    [MaxLength(140)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2400)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? ProjectUrl { get; set; }

    [Required]
    [MaxLength(40)]
    public string Status { get; set; } = "In progress";
}