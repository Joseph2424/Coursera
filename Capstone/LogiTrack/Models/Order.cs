namespace LogiTrack.Models;

public sealed class Order
{
    public int Id { get; set; }
    public required string CustomerName { get; set; }
    public required string CustomerEmail { get; set; }
    public int FulfillmentCenterId { get; set; }
    public FulfillmentCenter? FulfillmentCenter { get; set; }
    public OrderStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<OrderLine> Items { get; set; } = [];
}

public sealed class OrderLine
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public int InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Cancelled
}