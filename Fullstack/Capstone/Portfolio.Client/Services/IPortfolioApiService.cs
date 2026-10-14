using Portfolio.Client.Models;

namespace Portfolio.Client.Services;

public interface IPortfolioApiService
{
    Task<AuthResponseDto> RegisterAsync(AuthForm form, CancellationToken cancellationToken = default);

    Task<AuthResponseDto> LoginAsync(AuthForm form, CancellationToken cancellationToken = default);

    Task<AuthenticatedUserDto?> GetCurrentUserAsync(CancellationToken cancellationToken = default);

    Task LogoutAsync(CancellationToken cancellationToken = default);

    Task<List<PortfolioUserDto>> GetUsersAsync(CancellationToken cancellationToken = default);

    Task<List<ProjectDto>> GetProjectsAsync(int portfolioUserId, CancellationToken cancellationToken = default);

    Task<PortfolioUserDto> CreateUserAsync(PortfolioUserForm form, CancellationToken cancellationToken = default);

    Task<PortfolioUserDto> UpdateUserAsync(int id, PortfolioUserForm form, CancellationToken cancellationToken = default);

    Task<ProjectDto> CreateProjectAsync(int portfolioUserId, ProjectForm form, CancellationToken cancellationToken = default);

    Task<ProjectDto> UpdateProjectAsync(int id, ProjectForm form, CancellationToken cancellationToken = default);

    Task DeleteProjectAsync(int id, CancellationToken cancellationToken = default);
}