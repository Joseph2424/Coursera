using Portfolio.Client.Models;

namespace Portfolio.Client.Services;

public sealed class AuthStateService(IPortfolioApiService portfolioApi) : IAuthStateService
{
    private Task? _initializationTask;

    public AuthenticatedUserDto? CurrentUser { get; private set; }

    public bool IsAuthenticated => CurrentUser is not null;

    public bool IsLoading { get; private set; } = true;

    public event Action? StateChanged;

    public Task InitializeAsync(CancellationToken cancellationToken = default) =>
        _initializationTask ??= InitializeCoreAsync(cancellationToken);

    public async Task RegisterAsync(AuthForm form, CancellationToken cancellationToken = default)
    {
        var response = await portfolioApi.RegisterAsync(form, cancellationToken);
        SetCurrentUser(response.User);
    }

    public async Task LoginAsync(AuthForm form, CancellationToken cancellationToken = default)
    {
        var response = await portfolioApi.LoginAsync(form, cancellationToken);
        SetCurrentUser(response.User);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await portfolioApi.LogoutAsync(cancellationToken);
        }
        finally
        {
            SetCurrentUser(null);
        }
    }

    private async Task InitializeCoreAsync(CancellationToken cancellationToken)
    {
        try
        {
            SetCurrentUser(await portfolioApi.GetCurrentUserAsync(cancellationToken));
        }
        finally
        {
            IsLoading = false;
            StateChanged?.Invoke();
        }
    }

    private void SetCurrentUser(AuthenticatedUserDto? user)
    {
        CurrentUser = user;
        IsLoading = false;
        StateChanged?.Invoke();
    }
}