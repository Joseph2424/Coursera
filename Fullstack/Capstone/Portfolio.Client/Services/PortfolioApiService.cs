using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.JSInterop;
using Portfolio.Client.Models;

namespace Portfolio.Client.Services;

public sealed class PortfolioApiService(HttpClient httpClient, IJSRuntime jsRuntime) : IPortfolioApiService
{
    private const string TokenStorageKey = "portfolio.access-token";

    public Task<AuthResponseDto> RegisterAsync(AuthForm form, CancellationToken cancellationToken = default) =>
        AuthenticateAsync(new HttpRequestMessage(HttpMethod.Post, "api/identity/register")
        {
            Content = JsonContent.Create(form)
        }, cancellationToken);

    public Task<AuthResponseDto> LoginAsync(AuthForm form, CancellationToken cancellationToken = default) =>
        AuthenticateAsync(new HttpRequestMessage(HttpMethod.Post, "api/identity/login")
        {
            Content = JsonContent.Create(form)
        }, cancellationToken);

    public async Task<AuthenticatedUserDto?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var token = await ReadTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            return await SendAsync<AuthenticatedUserDto>(
                new HttpRequestMessage(HttpMethod.Get, "api/identity/me"),
                cancellationToken);
        }
        catch (PortfolioApiException exception) when (exception.StatusCode == HttpStatusCode.Unauthorized)
        {
            await LogoutAsync(cancellationToken);
            return null;
        }
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default) =>
        await jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", cancellationToken, TokenStorageKey);

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

    private async Task<AuthResponseDto> AuthenticateAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await SendAsync<AuthResponseDto>(request, cancellationToken, includeToken: false);
        await jsRuntime.InvokeVoidAsync("sessionStorage.setItem", cancellationToken, TokenStorageKey, response.AccessToken);
        return response;
    }

    private async Task<T> SendAsync<T>(
        HttpRequestMessage request,
        CancellationToken cancellationToken,
        bool includeToken = true)
    {
        if (includeToken)
        {
            var token = await ReadTokenAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

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

    private ValueTask<string?> ReadTokenAsync(CancellationToken cancellationToken) =>
        jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", cancellationToken, TokenStorageKey);
}

public sealed class PortfolioApiException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}