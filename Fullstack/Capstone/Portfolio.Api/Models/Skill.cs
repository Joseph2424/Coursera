using System.ComponentModel.DataAnnotations;

namespace Portfolio.Api.Models;

public sealed class Skill
{
    public int Id { get; set; }

    [Required]
    [MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    public ICollection<PortfolioUser> PortfolioUsers { get; set; } = new List<PortfolioUser>();
}