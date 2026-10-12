using System.Data.Common;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using LogiTrack.Contracts;
using LogiTrack.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace LogiTrack.IntegrationTests;

public sealed class SystemIntegrationTests
{
    internal const string AdminEmail = "admin@example.test";
    internal const string AdminPassword = "Integration-Admin-2026!";
    private const string UserEmail = "user@example.test";
    private const string UserPassword = "Integration-User-2026!";
    private static readonly JsonSerializerOptions ApiJsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
    private readonly ITestOutputHelper output;

    public SystemIntegrationTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public async Task InventoryAndOrderCreationReserveStock()
    {
        using var dataDirectory = new TestDataDirectory();
        using var factory = new TestApiFactory(dataDirectory.Path);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await GetBearerTokenAsync(client, AdminEmail, AdminPassword);

        var inventoryResponse = await client.PostAsJsonAsync("/api/inventory", new
        {
            sku = "TEST-BOX",
            name = "Test Shipping Box",
            fulfillmentCenterId = 1,
            quantityOnHand = 15,
            reorderLevel = 3,
            unitPrice = 4.25m
        });

        Assert.Equal(HttpStatusCode.Created, inventoryResponse.StatusCode);
        var inventory = await inventoryResponse.Content.ReadFromJsonAsync<InventoryItemResponse>();
        Assert.NotNull(inventory);

        var orderResponse = await client.PostAsJsonAsync("/api/orders", new
        {
            customerName = "Taylor Example",
            customerEmail = "taylor@example.test",
            fulfillmentCenterId = inventory.FulfillmentCenterId,
            items = new[] { new { inventoryItemId = inventory.Id, quantity = 4 } }
        });

        Assert.Equal(HttpStatusCode.Created, orderResponse.StatusCode);
        var order = await orderResponse.Content.ReadFromJsonAsync<OrderResponse>(ApiJsonOptions);
        Assert.NotNull(order);
        Assert.Single(order.Items);
        Assert.Equal(4, order.Items[0].Quantity);

        var inventoryAfterOrder = await client.GetFromJsonAsync<InventoryItemResponse>($"/api/inventory/{inventory.Id}");
        Assert.NotNull(inventoryAfterOrder);
        Assert.Equal(11, inventoryAfterOrder.QuantityOnHand);
    }

    [Fact]
    public async Task RegistrationAndLoginEnforceRoleBoundaries()
    {
        using var dataDirectory = new TestDataDirectory();
        using var factory = new TestApiFactory(dataDirectory.Path);
        using var client = factory.CreateClient();

        var unauthenticatedRead = await client.GetAsync("/api/inventory");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthenticatedRead.StatusCode);

        var registration = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = UserEmail,
            password = UserPassword
        });
        Assert.Equal(HttpStatusCode.OK, registration.StatusCode);

        client.DefaultRequestHeaders.Authorization = await GetBearerTokenAsync(client, UserEmail, UserPassword);
        var userRead = await client.GetAsync("/api/inventory");
        Assert.Equal(HttpStatusCode.OK, userRead.StatusCode);

        var userWrite = await client.PutAsJsonAsync("/api/inventory/1", new
        {
            sku = "SKU-100",
            name = "Wireless Scanner",
            fulfillmentCenterId = 1,
            quantityOnHand = 10,
            reorderLevel = 5,
            unitPrice = 79.99m
        });
        Assert.Equal(HttpStatusCode.Forbidden, userWrite.StatusCode);

        var userOrderRead = await client.GetAsync("/api/orders/1");
        Assert.Equal(HttpStatusCode.Forbidden, userOrderRead.StatusCode);

        client.DefaultRequestHeaders.Authorization = await GetBearerTokenAsync(client, AdminEmail, AdminPassword);
        var adminOrderRead = await client.GetAsync("/api/orders/1");
        Assert.Equal(HttpStatusCode.OK, adminOrderRead.StatusCode);
    }

    [Fact]
    public async Task InvalidInputReturnsClientErrors()
    {
        using var dataDirectory = new TestDataDirectory();
        using var factory = new TestApiFactory(dataDirectory.Path);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await GetBearerTokenAsync(client, AdminEmail, AdminPassword);

        var invalidInventory = await client.PostAsJsonAsync("/api/inventory", new
        {
            sku = "INVALID",
            name = "Invalid item",
            fulfillmentCenterId = 1,
            quantityOnHand = -1,
            reorderLevel = 0,
            unitPrice = 1m
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalidInventory.StatusCode);

        var insufficientStockOrder = await client.PostAsJsonAsync("/api/orders", new
        {
            customerName = "Taylor Example",
            customerEmail = "taylor@example.test",
            fulfillmentCenterId = 1,
            items = new[] { new { inventoryItemId = 1, quantity = 10000 } }
        });
        Assert.Equal(HttpStatusCode.Conflict, insufficientStockOrder.StatusCode);
        Assert.Equal("application/problem+json", insufficientStockOrder.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task CacheAvoidsRepeatedQueriesAndInvalidatesAfterInventoryUpdate()
    {
        using var dataDirectory = new TestDataDirectory();
        using var factory = new TestApiFactory(dataDirectory.Path);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await GetBearerTokenAsync(client, AdminEmail, AdminPassword);
        var queryCounter = factory.Services.GetRequiredService<SqlSelectCounter>();

        queryCounter.Reset();
        var coldTimer = Stopwatch.StartNew();
        var coldResponse = await client.GetAsync("/api/inventory");
        coldTimer.Stop();
        Assert.Equal(HttpStatusCode.OK, coldResponse.StatusCode);
        var coldSelects = queryCounter.Count;
        Assert.True(coldSelects > 0);

        queryCounter.Reset();
        var warmTimer = Stopwatch.StartNew();
        var warmResponse = await client.GetAsync("/api/inventory");
        warmTimer.Stop();
        Assert.Equal(HttpStatusCode.OK, warmResponse.StatusCode);
        var warmSelects = queryCounter.Count;
        Assert.Equal(0, warmSelects);
        output.WriteLine($"Inventory list: cold {coldTimer.Elapsed.TotalMilliseconds:F2} ms/{coldSelects} SELECTs; warm {warmTimer.Elapsed.TotalMilliseconds:F2} ms/{warmSelects} SELECTs.");

        var cachedOrders = await client.GetFromJsonAsync<List<OrderResponse>>("/api/orders", ApiJsonOptions);
        Assert.NotNull(cachedOrders);
        Assert.Contains(cachedOrders, order => order.Items.Any(line => line.InventoryItemId == 1));

        var updateResponse = await client.PutAsJsonAsync("/api/inventory/1", new
        {
            sku = "SKU-100",
            name = "Wireless Scanner Updated",
            fulfillmentCenterId = 1,
            quantityOnHand = 17,
            reorderLevel = 5,
            unitPrice = 79.99m
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        queryCounter.Reset();
        var updatedInventory = await client.GetFromJsonAsync<List<InventoryItemResponse>>("/api/inventory");
        Assert.NotNull(updatedInventory);
        Assert.Equal(17, Assert.Single(updatedInventory, item => item.Id == 1).QuantityOnHand);
        Assert.True(queryCounter.Count > 0);

        var refreshedOrders = await client.GetFromJsonAsync<List<OrderResponse>>("/api/orders", ApiJsonOptions);
        Assert.NotNull(refreshedOrders);
        Assert.Contains(refreshedOrders.SelectMany(order => order.Items), line =>
            line.InventoryItemId == 1 && line.Name == "Wireless Scanner Updated");
    }

    [Fact]
    public async Task InventoryAndOrdersPersistAcrossApplicationRestart()
    {
        using var dataDirectory = new TestDataDirectory();
        int inventoryId;
        int orderId;

        using (var firstFactory = new TestApiFactory(dataDirectory.Path))
        using (var firstClient = firstFactory.CreateClient())
        {
            firstClient.DefaultRequestHeaders.Authorization = await GetBearerTokenAsync(firstClient, AdminEmail, AdminPassword);
            var inventoryResponse = await firstClient.PostAsJsonAsync("/api/inventory", new
            {
                sku = "PERSIST-1",
                name = "Persistent Item",
                fulfillmentCenterId = 1,
                quantityOnHand = 9,
                reorderLevel = 2,
                unitPrice = 12m
            });
            Assert.Equal(HttpStatusCode.Created, inventoryResponse.StatusCode);
            var inventory = await inventoryResponse.Content.ReadFromJsonAsync<InventoryItemResponse>();
            Assert.NotNull(inventory);
            inventoryId = inventory.Id;

            var orderResponse = await firstClient.PostAsJsonAsync("/api/orders", new
            {
                customerName = "Persistence Check",
                customerEmail = "persistence@example.test",
                fulfillmentCenterId = 1,
                items = new[] { new { inventoryItemId = inventoryId, quantity = 2 } }
            });
            Assert.Equal(HttpStatusCode.Created, orderResponse.StatusCode);
            var order = await orderResponse.Content.ReadFromJsonAsync<OrderResponse>(ApiJsonOptions);
            Assert.NotNull(order);
            orderId = order.Id;
        }

        using var secondFactory = new TestApiFactory(dataDirectory.Path);
        using var secondClient = secondFactory.CreateClient();
        secondClient.DefaultRequestHeaders.Authorization = await GetBearerTokenAsync(secondClient, AdminEmail, AdminPassword);

        var persistedItem = await secondClient.GetFromJsonAsync<InventoryItemResponse>($"/api/inventory/{inventoryId}");
        var persistedOrder = await secondClient.GetFromJsonAsync<OrderResponse>($"/api/orders/{orderId}", ApiJsonOptions);
        Assert.NotNull(persistedItem);
        Assert.Equal(7, persistedItem.QuantityOnHand);
        Assert.NotNull(persistedOrder);
        Assert.Equal("Persistence Check", persistedOrder.CustomerName);
    }

    private static async Task<AuthenticationHeaderValue> GetBearerTokenAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(login);
        return new AuthenticationHeaderValue("Bearer", login.AccessToken);
    }
}

internal sealed class TestApiFactory(string dataDirectory) : WebApplicationFactory<Program>
{
    public SqlSelectCounter QueryCounter { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:LogiTrack"] = $"Data Source={Path.Combine(dataDirectory, "business.db")}",
                ["ConnectionStrings:Identity"] = $"Data Source={Path.Combine(dataDirectory, "identity.db")}",
                ["BootstrapAdmin:Email"] = SystemIntegrationTests.AdminEmail,
                ["BootstrapAdmin:Password"] = SystemIntegrationTests.AdminPassword,
                ["Authentication:SigningKey"] = "integration-test-signing-key-with-64-characters-long",
                ["Authentication:Issuer"] = "LogiTrack.Tests",
                ["Authentication:Audience"] = "LogiTrack.Tests.Api"
            }));

        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton(QueryCounter);
            services.AddDbContext<LogiTrackDbContext>((serviceProvider, options) =>
                options.AddInterceptors(serviceProvider.GetRequiredService<SqlSelectCounter>()));
        });
    }
}

internal sealed class SqlSelectCounter : DbCommandInterceptor
{
    private int count;

    public int Count => Volatile.Read(ref count);

    public void Reset() => Interlocked.Exchange(ref count, 0);

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        if (command.CommandText.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            Interlocked.Increment(ref count);
        return ValueTask.FromResult(result);
    }
}

internal sealed class TestDataDirectory : IDisposable
{
    public string Path { get; } = System.IO.Path.Combine(
        System.IO.Path.GetTempPath(), "LogiTrack.IntegrationTests", Guid.NewGuid().ToString("N"));

    public TestDataDirectory()
    {
        Directory.CreateDirectory(Path);
    }

    public void Dispose()
    {
        if (Directory.Exists(Path))
            Directory.Delete(Path, recursive: true);
    }
}