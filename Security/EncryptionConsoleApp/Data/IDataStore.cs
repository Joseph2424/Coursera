namespace EncryptionConsoleApp.Data;

public interface IDataStore
{
    void Save(string value);
    string GetSensitiveData(string role);
}
