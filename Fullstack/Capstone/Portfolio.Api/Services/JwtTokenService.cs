using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Portfolio.Api.Contracts;
using Portfolio.Api.Models;

namespace Portfolio.Api.Services;

public sealed class JwtTokenService(IOptions<JwtTokenOptions> options) : IJwtTokenService
{
    public AuthResponse CreateToken(ApplicationUser user)
    {
        var settings = options.Value;
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(settings.LifetimeMinutes);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
        };
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new AuthResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt,
            new AuthenticatedUserDto(user.Id, user.Email ?? string.Empty));
    }
}