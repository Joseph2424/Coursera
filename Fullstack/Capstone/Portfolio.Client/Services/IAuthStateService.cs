using Portfolio.Client.Models;

namespace Portfolio.Client.Services;

public interface IAuthStateService
{
    AuthenticatedUserDto? CurrentUser { get; }

    bool IsAuthenticated { get; }

    bool IsLoading { get; }

    event Action? StateChanged;

    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task RegisterAsync(AuthForm form, CancellationToken cancellationToken = default);

    Task LoginAsync(AuthForm form, CancellationToken cancellationToken = default);

    Task LogoutAsync(CancellationToken cancellationToken = default);
}