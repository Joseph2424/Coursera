using LogiTrack.Contracts;
using LogiTrack.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LogiTrack.Endpoints;

public static class FulfillmentCenterEndpoints
{
    public static void MapFulfillmentCenterEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/fulfillment-centers").WithTags("Fulfillment Centers");
        group.MapGet("/", async Task<Ok<IReadOnlyList<FulfillmentCenterResponse>>>(IOrderManagementService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetFulfillmentCentersAsync(ct)))
            .WithName("GetFulfillmentCenters").WithSummary("List fulfillment centers");
        group.MapPost("/", async Task<Created<FulfillmentCenterResponse>>(CreateFulfillmentCenterRequest request, IOrderManagementService service, CancellationToken ct) =>
        {
            var center = await service.CreateFulfillmentCenterAsync(request, ct);
            return TypedResults.Created($"/api/fulfillment-centers/{center.Id}", center);
        }).WithName("CreateFulfillmentCenter").WithSummary("Create a fulfillment center");
    }
}