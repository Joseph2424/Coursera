namespace Portfolio.Api.Models;

public sealed class PortfolioUserSkill
{
    public int PortfolioUsersId { get; set; }

    public PortfolioUser PortfolioUser { get; set; } = null!;

    public int SkillsId { get; set; }

    public Skill Skill { get; set; } = null!;
}