using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using InventoryManagement.Application.DTOs.Product;

namespace InventoryManagement.Web.Services;

public class InventoryApiClient(HttpClient httpClient)
{
    private static readonly TimeSpan ApiRequestTimeout = TimeSpan.FromSeconds(2);

    private readonly HttpClient _httpClient = httpClient;

    public Task<InventoryApiResult<GetProduct>> GetProductByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            return Task.FromResult(InventoryApiResult<GetProduct>.NotFound());
        }

        return GetMaybeNotFoundAsync<GetProduct>(
            $"product/single/{id}",
            ApiRequestTimeout,
            cancellationToken
        );
    }

    private async Task<InventoryApiResult<T>> GetMaybeNotFoundAsync<T>(
        string route,
        TimeSpan requestTimeout,
        CancellationToken cancellationToken
    )
    {
        using var requestTimeoutToken = CreateRequestTimeoutToken(
            requestTimeout,
            cancellationToken
        );

        try
        {
            using var response = await _httpClient.GetAsync(route, requestTimeoutToken.Token);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return InventoryApiResult<T>.NotFound();
            }

            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<T>(
                cancellationToken: cancellationToken
            );
            return payload is not null
                ? InventoryApiResult<T>.Success(payload)
                : InventoryApiResult<T>.NotFound();
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return InventoryApiResult<T>.ServiceUnavailable();
        }
        catch (Exception exception)
            when (exception is HttpRequestException or JsonException or NotSupportedException)
        {
            return InventoryApiResult<T>.ServiceUnavailable();
        }
    }

    private static CancellationTokenSource CreateRequestTimeoutToken(
        TimeSpan requestTimeout,
        CancellationToken cancellationToken
    )
    {
        var requestTimeoutToken = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken
        );
        requestTimeoutToken.CancelAfter(requestTimeout);
        return requestTimeoutToken;
    }
}
