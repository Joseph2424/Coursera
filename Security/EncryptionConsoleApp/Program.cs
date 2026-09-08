using EncryptionConsoleApp.Data;
using EncryptionConsoleApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<IAesEncryptionService, AesEncryptionService>();

        services.AddSingleton<IAuthorizationService, RoleAuthorizationService>();

        services.AddSingleton<IDataStore, DataStore>();
    })
    .Build();

var store = host.Services.GetRequiredService<IDataStore>();

Console.Write("Enter role (Admin/User): ");
string role = Console.ReadLine() ?? "User";

Console.Write("Enter sensitive data: ");
string data = Console.ReadLine() ?? string.Empty;

store.Save(data);

Console.WriteLine();
Console.WriteLine("Encrypted data stored in memory.");

Console.WriteLine();

try
{
    string sensitiveData = store.GetSensitiveData(role);

    Console.WriteLine($"Sensitive Data: {sensitiveData}");
}
catch (UnauthorizedAccessException ex)
{
    Console.WriteLine(ex.Message);
}
