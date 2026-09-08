using System.Security.Cryptography;
using System.Text;
using EncryptionConsoleApp.Models;
using Microsoft.Extensions.Logging;

namespace EncryptionConsoleApp.Services;

public sealed class AesEncryptionService : IAesEncryptionService
{
    private readonly ILogger<AesEncryptionService> _logger;
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public AesEncryptionService(ILogger<AesEncryptionService> logger)
    {
        _logger = logger;

        using var aes = Aes.Create();

        aes.KeySize = 256;
        aes.GenerateKey();
        aes.GenerateIV();

        _key = aes.Key;
        _iv = aes.IV;

        _logger.LogInformation("AES encryption service initialized with new key and IV.");
    }

    public EncryptedData Encrypt(string plainText)
    {
        _logger.LogInformation("Encryption requested.");

        using var aes = Aes.Create();

        aes.Key = _key;
        aes.IV = _iv;

        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        using var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using var writer = new StreamWriter(cryptoStream, Encoding.UTF8);

        writer.Write(plainText);
        writer.Flush();
        cryptoStream.FlushFinalBlock();

        var cipherText = Convert.ToBase64String(ms.ToArray());

        _logger.LogInformation("Encryption completed successfully.");

        return new EncryptedData(cipherText);
    }

    public string Decrypt(EncryptedData encryptedData)
    {
        _logger.LogInformation("Decryption requested.");

        byte[] cipherBytes = Convert.FromBase64String(encryptedData.CipherText);

        using var aes = Aes.Create();

        aes.Key = _key;
        aes.IV = _iv;

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(cipherBytes);
        using var cryptoStream = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var reader = new StreamReader(cryptoStream);

        var result = reader.ReadToEnd();

        _logger.LogInformation("Decryption completed successfully.");

        return result;
    }
}
