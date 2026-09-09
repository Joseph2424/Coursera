using SecurePortal.Api.Identity;

namespace SecurePortal.Api.Services;

public interface IJwtTokenGenerator
{
    Task<string> CreateTokenAsync(ApplicationUser user);
}