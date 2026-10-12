using LogiTrack.Contracts;
using LogiTrack.Services;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.Controllers;

[ApiController]
[Route("api/fulfillment-centers")]
[Tags("Fulfillment Centers")]
public sealed class FulfillmentCentersController(IOrderManagementService service) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List fulfillment centers")]
    [ProducesResponseType(typeof(IReadOnlyList<FulfillmentCenterResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FulfillmentCenterResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetFulfillmentCentersAsync(cancellationToken));

    [HttpGet("{id:int}")]
    [EndpointSummary("Get a fulfillment center")]
    [ProducesResponseType(typeof(FulfillmentCenterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FulfillmentCenterResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var center = await service.GetFulfillmentCenterAsync(id, cancellationToken);
        return center is null ? NotFound() : Ok(center);
    }

    [HttpPost]
    [EndpointSummary("Create a fulfillment center")]
    [ProducesResponseType(typeof(FulfillmentCenterResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<FulfillmentCenterResponse>> Create(
        CreateFulfillmentCenterRequest request,
        CancellationToken cancellationToken)
    {
        var center = await service.CreateFulfillmentCenterAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = center.Id }, center);
    }
}