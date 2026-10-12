using LogiTrack.Contracts;
using LogiTrack.Security;
using LogiTrack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.Controllers;

[Authorize(Roles = AppRoles.Admin)]
[ApiController]
[Route("api/orders")]
[Tags("Orders")]
public sealed class OrdersController(IOrderManagementService service) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List customer orders")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetOrdersAsync(cancellationToken));

    [HttpGet("{id:int}")]
    [EndpointSummary("Get a customer order")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var order = await service.GetOrderAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    [EndpointSummary("Create an order and reserve inventory")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<OrderResponse>> Create(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var order = await service.CreateOrderAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPut("{id:int}")]
    [EndpointSummary("Update order details or status")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> Update(
        int id,
        UpdateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var order = await service.UpdateOrderAsync(id, request, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }
}