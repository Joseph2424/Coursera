using System.Net;
using System.Net.Http.Json;
using Portfolio.Client.Models;

namespace Portfolio.Client.Services;

public sealed class PortfolioApiService(HttpClient httpClient) : IPortfolioApiService
{
    public Task<List<PortfolioUserDto>> GetUsersAsync(CancellationToken cancellationToken = default) =>
        SendAsync<List<PortfolioUserDto>>(new HttpRequestMessage(HttpMethod.Get, "api/portfolio-users"), cancellationToken);

    public Task<List<ProjectDto>> GetProjectsAsync(int portfolioUserId, CancellationToken cancellationToken = default) =>
        SendAsync<List<ProjectDto>>(
            new HttpRequestMessage(HttpMethod.Get, $"api/projects?portfolioUserId={portfolioUserId}"),
            cancellationToken);

    public Task<PortfolioUserDto> CreateUserAsync(PortfolioUserForm form, CancellationToken cancellationToken = default) =>
        SendAsync<PortfolioUserDto>(new HttpRequestMessage(HttpMethod.Post, "api/portfolio-users")
        {
            Content = JsonContent.Create(form)
        }, cancellationToken);

    public Task<PortfolioUserDto> UpdateUserAsync(
        int id,
        PortfolioUserForm form,
        CancellationToken cancellationToken = default) =>
        SendAsync<PortfolioUserDto>(new HttpRequestMessage(HttpMethod.Put, $"api/portfolio-users/{id}")
        {
            Content = JsonContent.Create(form)
        }, cancellationToken);

    public Task<ProjectDto> CreateProjectAsync(
        int portfolioUserId,
        ProjectForm form,
        CancellationToken cancellationToken = default) =>
        SendAsync<ProjectDto>(new HttpRequestMessage(HttpMethod.Post, $"api/projects?portfolioUserId={portfolioUserId}")
        {
            Content = JsonContent.Create(form)
        }, cancellationToken);

    public Task<ProjectDto> UpdateProjectAsync(
        int id,
        ProjectForm form,
        CancellationToken cancellationToken = default) =>
        SendAsync<ProjectDto>(new HttpRequestMessage(HttpMethod.Put, $"api/projects/{id}")
        {
            Content = JsonContent.Create(form)
        }, cancellationToken);

    private async Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using (request)
        using (var response = await httpClient.SendAsync(request, cancellationToken))
        {
            if (!response.IsSuccessStatusCode)
            {
                var details = await response.Content.ReadAsStringAsync(cancellationToken);
                var message = string.IsNullOrWhiteSpace(details)
                    ? $"The API request failed with status {(int)response.StatusCode}."
                    : details;
                throw new PortfolioApiException(response.StatusCode, message);
            }

            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken)
                ?? throw new PortfolioApiException(HttpStatusCode.InternalServerError, "The API returned an empty response.");
        }
    }
}

public sealed class PortfolioApiException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}