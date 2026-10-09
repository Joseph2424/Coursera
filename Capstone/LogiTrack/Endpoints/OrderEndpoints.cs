using LogiTrack.Contracts;
using LogiTrack.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LogiTrack.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");
        group.MapGet("/", async Task<Ok<IReadOnlyList<OrderResponse>>>(IOrderManagementService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetOrdersAsync(ct)))
            .WithName("GetOrders").WithSummary("List customer orders");
        group.MapGet("/{id:int}", async Task<Results<Ok<OrderResponse>, NotFound>>(int id, IOrderManagementService service, CancellationToken ct) =>
        {
            var order = await service.GetOrderAsync(id, ct);
            return order is null ? TypedResults.NotFound() : TypedResults.Ok(order);
        }).WithName("GetOrder").WithSummary("Get a customer order");
        group.MapPost("/", async Task<Created<OrderResponse>>(CreateOrderRequest request, IOrderManagementService service, CancellationToken ct) =>
        {
            var order = await service.CreateOrderAsync(request, ct);
            return TypedResults.Created($"/api/orders/{order.Id}", order);
        }).WithName("CreateOrder").WithSummary("Create an order and reserve inventory");
        group.MapPut("/{id:int}", async Task<Results<Ok<OrderResponse>, NotFound>>(int id, UpdateOrderRequest request, IOrderManagementService service, CancellationToken ct) =>
        {
            var order = await service.UpdateOrderAsync(id, request, ct);
            return order is null ? TypedResults.NotFound() : TypedResults.Ok(order);
        }).WithName("UpdateOrder").WithSummary("Update order details or status");
    }
}