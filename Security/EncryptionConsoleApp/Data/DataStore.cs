using EncryptionConsoleApp.Models;
using EncryptionConsoleApp.Services;
using Microsoft.Extensions.Logging;

namespace EncryptionConsoleApp.Data;

public sealed class DataStore(
    IAesEncryptionService encryptionService,
    IAuthorizationService authorizationService,
    ILogger<DataStore> logger
) : IDataStore
{
    private readonly IAesEncryptionService _encryptionService = encryptionService;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ILogger<DataStore> _logger = logger;

    private EncryptedData? _encryptedData;

    public void Save(string value)
    {
        _logger.LogInformation("Storing sensitive data in memory.");

        _encryptedData = _encryptionService.Encrypt(value);
    }

    public string GetSensitiveData(string role)
    {
        if (!_authorizationService.CanAccessSensitiveData(role))
        {
            _logger.LogError("Unauthorized access attempt detected.");

            throw new UnauthorizedAccessException("Access denied.");
        }

        if (_encryptedData is null)
        {
            throw new InvalidOperationException("No data available.");
        }

        _logger.LogInformation("Sensitive data retrieved.");

        return _encryptionService.Decrypt(_encryptedData);
    }
}
