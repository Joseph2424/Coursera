namespace LogiTrack.Models;

public sealed class InventoryItem
{
    public int Id { get; set; }
    public required string Sku { get; set; }
    public required string Name { get; set; }
    public int FulfillmentCenterId { get; set; }
    public FulfillmentCenter? FulfillmentCenter { get; set; }
    public int QuantityOnHand { get; set; }
    public int ReorderLevel { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}