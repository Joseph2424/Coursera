namespace EncryptionConsoleApp.Services;

public interface IAuthorizationService
{
    bool CanAccessSensitiveData(string role);
}
