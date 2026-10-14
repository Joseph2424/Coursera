using Microsoft.AspNetCore.Identity;

namespace Portfolio.Api.Models;

public sealed class ApplicationUser : IdentityUser
{
    public ICollection<PortfolioUser> Portfolios { get; set; } = new List<PortfolioUser>();
}