namespace LogiTrack.Models;

public sealed class FulfillmentCenter
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required string Location { get; set; }
}