using EncryptionConsoleApp.Models;

namespace EncryptionConsoleApp.Services;

public interface IAesEncryptionService
{
    EncryptedData Encrypt(string plainText);
    string Decrypt(EncryptedData encryptedData);
}
