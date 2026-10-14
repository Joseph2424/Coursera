using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Portfolio.E2E.Tests;

internal sealed class E2EApplicationHost : IAsyncDisposable
{
    public const string ApiBaseUrl = "https://localhost:7021";
    public const string ClientBaseUrl = "https://localhost:7261";

    private const string JwtSigningKey = "Portfolio-E2E-Test-Key-Only-2026-Not-For-Production";
    private readonly ConcurrentQueue<string> _output = new();
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"portfolio-e2e-{Guid.NewGuid():N}.db");
    private Process? _apiProcess;
    private Process? _clientProcess;

    public async Task StartAsync()
    {
        EnsurePortIsAvailable(7021);
        EnsurePortIsAvailable(7261);

        _apiProcess = StartProcess(
            "Portfolio.Api/Portfolio.Api.csproj",
            "API",
            process =>
            {
                process.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
                process.Environment["ConnectionStrings__Portfolio"] = $"Data Source={_databasePath}";
                process.Environment["Jwt__Key"] = JwtSigningKey;
            });

        await WaitForReadyAsync($"{ApiBaseUrl}/openapi/v1.json", _apiProcess, "API");

        _clientProcess = StartProcess(
            "Portfolio.Client/Portfolio.Client.csproj",
            "Client",
            process => process.Environment["ASPNETCORE_ENVIRONMENT"] = "Development");

        await WaitForReadyAsync(ClientBaseUrl, _clientProcess, "Blazor client");
    }

    public async ValueTask DisposeAsync()
    {
        await StopProcessAsync(_clientProcess);
        await StopProcessAsync(_apiProcess);

        foreach (var suffix in new[] { string.Empty, "-shm", "-wal" })
        {
            try
            {
                File.Delete(_databasePath + suffix);
            }
            catch (IOException)
            {
            }
        }
    }

    private Process StartProcess(
        string projectPath,
        string name,
        Action<ProcessStartInfo> configure)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = FindRepositoryRoot(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("--project");
        startInfo.ArgumentList.Add(projectPath);
        startInfo.ArgumentList.Add("--launch-profile");
        startInfo.ArgumentList.Add("https");
        configure(startInfo);

        var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        process.OutputDataReceived += (_, eventArgs) => AddOutput(name, eventArgs.Data);
        process.ErrorDataReceived += (_, eventArgs) => AddOutput(name, eventArgs.Data);
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        return process;
    }

    private async Task WaitForReadyAsync(string url, Process process, string name)
    {
        using var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        using var client = new HttpClient(handler);
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));

        while (!timeout.IsCancellationRequested)
        {
            if (process.HasExited)
            {
                throw new InvalidOperationException($"{name} exited during startup.{Environment.NewLine}{GetOutput()}");
            }

            try
            {
                using var response = await client.GetAsync(url, timeout.Token);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
            }

            await Task.Delay(500, timeout.Token);
        }

        throw new TimeoutException($"{name} did not become ready at {url}.{Environment.NewLine}{GetOutput()}");
    }

    private static void EnsurePortIsAvailable(int port)
    {
        using var listener = new TcpListener(IPAddress.Loopback, port);
        try
        {
            listener.Start();
        }
        catch (SocketException exception)
        {
            throw new InvalidOperationException(
                $"Port {port} is already in use. Stop the existing app before running the E2E suite.",
                exception);
        }
    }

    private static async Task StopProcessAsync(Process? process)
    {
        if (process is null)
        {
            return;
        }

        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
            }
        }
        catch (InvalidOperationException)
        {
        }
        finally
        {
            process.Dispose();
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "PortfolioTracker.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("Could not find PortfolioTracker.sln from the test output directory.");
    }

    private void AddOutput(string name, string? line)
    {
        if (!string.IsNullOrWhiteSpace(line))
        {
            _output.Enqueue($"[{name}] {line}");
        }
    }

    private string GetOutput() => string.Join(Environment.NewLine, _output);
}