using System.Security.Cryptography;

namespace EncryptionWebApp.Services;

public interface IEncryptionService
{
    byte[] Encrypt(byte[] data);
    byte[] Decrypt(byte[] data);
}

public class EncryptionService(IConfiguration configuration) : IEncryptionService
{
    private readonly byte[] _key = Convert.FromBase64String(configuration["Encryption:Key"]!);
    private readonly byte[] _iv = Convert.FromBase64String(configuration["Encryption:IV"]!);

    public byte[] Encrypt(byte[] data)
    {
        using var aes = Aes.Create();

        aes.Key = _key;
        aes.IV = _iv;

        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();

        using (var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            cryptoStream.Write(data, 0, data.Length);
        }

        return ms.ToArray();
    }

    public byte[] Decrypt(byte[] encryptedData)
    {
        using var aes = Aes.Create();

        aes.Key = _key;
        aes.IV = _iv;

        using var decryptor = aes.CreateDecryptor();
        using var inputStream = new MemoryStream(encryptedData);
        using var cryptoStream = new CryptoStream(inputStream, decryptor, CryptoStreamMode.Read);
        using var outputStream = new MemoryStream();

        cryptoStream.CopyTo(outputStream);

        return outputStream.ToArray();
    }
}