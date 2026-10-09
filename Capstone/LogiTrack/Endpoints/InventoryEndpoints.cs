using LogiTrack.Contracts;
using LogiTrack.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LogiTrack.Endpoints;

public static class InventoryEndpoints
{
    public static void MapInventoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/inventory").WithTags("Inventory");
        group.MapGet("/", async Task<Ok<IReadOnlyList<InventoryItemResponse>>>(IOrderManagementService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetInventoryAsync(ct)))
            .WithName("GetInventory").WithSummary("List inventory across fulfillment centers");
        group.MapGet("/{id:int}", async Task<Results<Ok<InventoryItemResponse>, NotFound>>(int id, IOrderManagementService service, CancellationToken ct) =>
        {
            var item = await service.GetInventoryItemAsync(id, ct);
            return item is null ? TypedResults.NotFound() : TypedResults.Ok(item);
        }).WithName("GetInventoryItem").WithSummary("Get an inventory item");
        group.MapPost("/", async Task<Created<InventoryItemResponse>>(InventoryItemRequest request, IOrderManagementService service, CancellationToken ct) =>
        {
            var item = await service.CreateInventoryItemAsync(request, ct);
            return TypedResults.Created($"/api/inventory/{item.Id}", item);
        }).WithName("CreateInventoryItem").WithSummary("Create inventory at a fulfillment center");
        group.MapPut("/{id:int}", async Task<Results<Ok<InventoryItemResponse>, NotFound>>(int id, InventoryItemRequest request, IOrderManagementService service, CancellationToken ct) =>
        {
            var item = await service.UpdateInventoryItemAsync(id, request, ct);
            return item is null ? TypedResults.NotFound() : TypedResults.Ok(item);
        }).WithName("UpdateInventoryItem").WithSummary("Replace an inventory item");
    }
}