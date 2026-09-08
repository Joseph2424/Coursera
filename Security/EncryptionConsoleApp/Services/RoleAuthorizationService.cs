using Microsoft.Extensions.Logging;

namespace EncryptionConsoleApp.Services;

public sealed class RoleAuthorizationService(ILogger<RoleAuthorizationService> logger) : IAuthorizationService
{
    private readonly ILogger<RoleAuthorizationService> _logger = logger;

    public bool CanAccessSensitiveData(string role)
    {
        bool authorized = role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

        if (authorized)
        {
            _logger.LogInformation("Authorization granted for role {Role}.", role);
        }
        else
        {
            _logger.LogWarning("Authorization denied for role {Role}.", role);
        }

        return authorized;
    }
}
