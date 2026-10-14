using Portfolio.Api.Contracts;
using Portfolio.Api.Models;

namespace Portfolio.Api.Services;

public interface IJwtTokenService
{
    AuthResponse CreateToken(ApplicationUser user);
}