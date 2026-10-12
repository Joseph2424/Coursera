using LogiTrack.Contracts;
using LogiTrack.Security;
using LogiTrack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.Controllers;

[Authorize]
[ApiController]
[Route("api/inventory")]
[Tags("Inventory")]
public sealed class InventoryController(IOrderManagementService service) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List inventory across fulfillment centers")]
    [ProducesResponseType(typeof(IReadOnlyList<InventoryItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<InventoryItemResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetInventoryAsync(cancellationToken));

    [HttpGet("{id:int}")]
    [EndpointSummary("Get an inventory item")]
    [ProducesResponseType(typeof(InventoryItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventoryItemResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await service.GetInventoryItemAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    [EndpointSummary("Create inventory at a fulfillment center")]
    [ProducesResponseType(typeof(InventoryItemResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<InventoryItemResponse>> Create(
        InventoryItemRequest request,
        CancellationToken cancellationToken)
    {
        var item = await service.CreateInventoryItemAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    [EndpointSummary("Replace an inventory item")]
    [ProducesResponseType(typeof(InventoryItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventoryItemResponse>> Update(
        int id,
        InventoryItemRequest request,
        CancellationToken cancellationToken)
    {
        var item = await service.UpdateInventoryItemAsync(id, request, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }
}