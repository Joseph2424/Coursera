using System.ComponentModel.DataAnnotations;
using LogiTrack.Models;

namespace LogiTrack.Contracts;

/// <summary>Payload for creating a fulfillment center.</summary>
public sealed record CreateFulfillmentCenterRequest
{
    [Required, MaxLength(20)]
    public required string Code { get; init; }
    [Required, MaxLength(120)]
    public required string Name { get; init; }
    [Required, MaxLength(200)]
    public required string Location { get; init; }
}

/// <summary>A fulfillment center returned by the API.</summary>
public sealed record FulfillmentCenterResponse(int Id, string Code, string Name, string Location);

/// <summary>Payload for creating or replacing an inventory item.</summary>
public sealed record InventoryItemRequest
{
    [Required, MaxLength(64)]
    public required string Sku { get; init; }
    [Required, MaxLength(200)]
    public required string Name { get; init; }
    [Range(1, int.MaxValue)]
    public int FulfillmentCenterId { get; init; }
    [Range(0, int.MaxValue)]
    public int QuantityOnHand { get; init; }
    [Range(0, int.MaxValue)]
    public int ReorderLevel { get; init; }
    [Range(0, 99999999.99)]
    public decimal UnitPrice { get; init; }
}

/// <summary>An inventory item and its stock at one fulfillment center.</summary>
public sealed record InventoryItemResponse(
    int Id, string Sku, string Name, int FulfillmentCenterId, string FulfillmentCenterCode,
    int QuantityOnHand, int ReorderLevel, decimal UnitPrice, DateTimeOffset UpdatedAt);

/// <summary>One inventory item and quantity requested for an order.</summary>
public sealed record CreateOrderLineRequest
{
    [Range(1, int.MaxValue)]
    public int InventoryItemId { get; init; }
    [Range(1, int.MaxValue)]
    public int Quantity { get; init; }
}

/// <summary>Payload for creating a customer order.</summary>
public sealed record CreateOrderRequest
{
    [Required, MaxLength(200)]
    public required string CustomerName { get; init; }
    [Required, EmailAddress, MaxLength(254)]
    public required string CustomerEmail { get; init; }
    [Range(1, int.MaxValue)]
    public int FulfillmentCenterId { get; init; }
    [Required, MinLength(1)]
    public required List<CreateOrderLineRequest> Items { get; init; }
}

/// <summary>Payload for updating order customer details and status.</summary>
public sealed record UpdateOrderRequest
{
    [Required, MaxLength(200)]
    public required string CustomerName { get; init; }
    [Required, EmailAddress, MaxLength(254)]
    public required string CustomerEmail { get; init; }
    public OrderStatus Status { get; init; }
}

/// <summary>An item and quantity included in an order.</summary>
public sealed record OrderLineResponse(int InventoryItemId, string Sku, string Name, int Quantity, decimal UnitPrice);

/// <summary>A customer order with its fulfillment center and items.</summary>
public sealed record OrderResponse(
    int Id, string CustomerName, string CustomerEmail, int FulfillmentCenterId, string FulfillmentCenterCode,
    OrderStatus Status, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, IReadOnlyList<OrderLineResponse> Items);