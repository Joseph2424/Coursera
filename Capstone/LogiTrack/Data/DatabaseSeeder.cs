using LogiTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(LogiTrackDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (await dbContext.FulfillmentCenters.AnyAsync(cancellationToken))
            return;

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var seattle = new FulfillmentCenter { Code = "SEA", Name = "Seattle Fulfillment Center", Location = "Seattle, WA" };
        var dallas = new FulfillmentCenter { Code = "DAL", Name = "Dallas Fulfillment Center", Location = "Dallas, TX" };
        var newark = new FulfillmentCenter { Code = "EWR", Name = "Newark Fulfillment Center", Location = "Newark, NJ" };
        dbContext.FulfillmentCenters.AddRange(seattle, dallas, newark);
        await dbContext.SaveChangesAsync(cancellationToken);

        var seattleScanner = new InventoryItem
        {
            Sku = "SKU-100", Name = "Wireless Scanner", FulfillmentCenterId = seattle.Id,
            QuantityOnHand = 10, ReorderLevel = 5, UnitPrice = 79.99m, UpdatedAt = DateTimeOffset.UtcNow
        };
        var seattleLabels = new InventoryItem
        {
            Sku = "SKU-200", Name = "Shipping Label Roll", FulfillmentCenterId = seattle.Id,
            QuantityOnHand = 40, ReorderLevel = 20, UnitPrice = 8.50m, UpdatedAt = DateTimeOffset.UtcNow
        };
        var dallasScanner = new InventoryItem
        {
            Sku = "SKU-100", Name = "Wireless Scanner", FulfillmentCenterId = dallas.Id,
            QuantityOnHand = 6, ReorderLevel = 5, UnitPrice = 79.99m, UpdatedAt = DateTimeOffset.UtcNow
        };
        var dallasPrinter = new InventoryItem
        {
            Sku = "SKU-300", Name = "Thermal Printer", FulfillmentCenterId = dallas.Id,
            QuantityOnHand = 4, ReorderLevel = 3, UnitPrice = 189.00m, UpdatedAt = DateTimeOffset.UtcNow
        };
        var newarkLabels = new InventoryItem
        {
            Sku = "SKU-200", Name = "Shipping Label Roll", FulfillmentCenterId = newark.Id,
            QuantityOnHand = 28, ReorderLevel = 20, UnitPrice = 8.50m, UpdatedAt = DateTimeOffset.UtcNow
        };
        dbContext.InventoryItems.AddRange(seattleScanner, seattleLabels, dallasScanner, dallasPrinter, newarkLabels);
        await dbContext.SaveChangesAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var orders = new[]
        {
            new Order
            {
                CustomerName = "Dana Morris", CustomerEmail = "dana@example.com",
                FulfillmentCenterId = seattle.Id, Status = OrderStatus.Pending,
                CreatedAt = now.AddDays(-3), UpdatedAt = now.AddDays(-3),
                Items =
                [
                    new OrderLine { InventoryItemId = seattleScanner.Id, Quantity = 2, UnitPrice = seattleScanner.UnitPrice },
                    new OrderLine { InventoryItemId = seattleLabels.Id, Quantity = 5, UnitPrice = seattleLabels.UnitPrice }
                ]
            },
            new Order
            {
                CustomerName = "Mateo Rivera", CustomerEmail = "mateo@example.com",
                FulfillmentCenterId = dallas.Id, Status = OrderStatus.Processing,
                CreatedAt = now.AddDays(-2), UpdatedAt = now.AddDays(-1),
                Items =
                [
                    new OrderLine { InventoryItemId = dallasScanner.Id, Quantity = 1, UnitPrice = dallasScanner.UnitPrice }
                ]
            },
            new Order
            {
                CustomerName = "Amara Brooks", CustomerEmail = "amara@example.com",
                FulfillmentCenterId = newark.Id, Status = OrderStatus.Shipped,
                CreatedAt = now.AddDays(-1), UpdatedAt = now,
                Items =
                [
                    new OrderLine { InventoryItemId = newarkLabels.Id, Quantity = 4, UnitPrice = newarkLabels.UnitPrice }
                ]
            }
        };
        dbContext.Orders.AddRange(orders);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}