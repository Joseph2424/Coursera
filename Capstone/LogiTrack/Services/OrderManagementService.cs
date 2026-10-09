using LogiTrack.Contracts;
using LogiTrack.Data;
using LogiTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Services;

public sealed class OrderManagementService(LogiTrackDbContext dbContext) : IOrderManagementService
{
    public async Task<IReadOnlyList<FulfillmentCenterResponse>> GetFulfillmentCentersAsync(CancellationToken cancellationToken) =>
        await dbContext.FulfillmentCenters.AsNoTracking().OrderBy(center => center.Code)
            .Select(center => new FulfillmentCenterResponse(center.Id, center.Code, center.Name, center.Location))
            .ToListAsync(cancellationToken);

    public async Task<FulfillmentCenterResponse> CreateFulfillmentCenterAsync(CreateFulfillmentCenterRequest request, CancellationToken cancellationToken)
    {
        var center = new FulfillmentCenter { Code = request.Code.Trim(), Name = request.Name.Trim(), Location = request.Location.Trim() };
        dbContext.FulfillmentCenters.Add(center);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new FulfillmentCenterResponse(center.Id, center.Code, center.Name, center.Location);
    }

    public async Task<IReadOnlyList<InventoryItemResponse>> GetInventoryAsync(CancellationToken cancellationToken) =>
        await dbContext.InventoryItems.AsNoTracking().Include(item => item.FulfillmentCenter)
            .OrderBy(item => item.Sku).ThenBy(item => item.FulfillmentCenter!.Code)
            .Select(item => ToInventoryResponse(item)).ToListAsync(cancellationToken);

    public async Task<InventoryItemResponse?> GetInventoryItemAsync(int id, CancellationToken cancellationToken)
    {
        var item = await dbContext.InventoryItems.AsNoTracking().Include(row => row.FulfillmentCenter)
            .SingleOrDefaultAsync(row => row.Id == id, cancellationToken);
        return item is null ? null : ToInventoryResponse(item);
    }

    public async Task<InventoryItemResponse> CreateInventoryItemAsync(InventoryItemRequest request, CancellationToken cancellationToken)
    {
        var item = new InventoryItem
        {
            Sku = request.Sku.Trim(), Name = request.Name.Trim(), FulfillmentCenterId = request.FulfillmentCenterId,
            QuantityOnHand = request.QuantityOnHand, ReorderLevel = request.ReorderLevel,
            UnitPrice = request.UnitPrice, UpdatedAt = DateTimeOffset.UtcNow
        };
        dbContext.InventoryItems.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);
        await dbContext.Entry(item).Reference(row => row.FulfillmentCenter).LoadAsync(cancellationToken);
        return ToInventoryResponse(item);
    }

    public async Task<InventoryItemResponse?> UpdateInventoryItemAsync(int id, InventoryItemRequest request, CancellationToken cancellationToken)
    {
        var item = await dbContext.InventoryItems.SingleOrDefaultAsync(row => row.Id == id, cancellationToken);
        if (item is null)
            return null;
        item.Sku = request.Sku.Trim();
        item.Name = request.Name.Trim();
        item.FulfillmentCenterId = request.FulfillmentCenterId;
        item.QuantityOnHand = request.QuantityOnHand;
        item.ReorderLevel = request.ReorderLevel;
        item.UnitPrice = request.UnitPrice;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        await dbContext.Entry(item).Reference(row => row.FulfillmentCenter).LoadAsync(cancellationToken);
        return ToInventoryResponse(item);
    }

    public async Task<IReadOnlyList<OrderResponse>> GetOrdersAsync(CancellationToken cancellationToken) =>
        await dbContext.Orders.AsNoTracking().Include(order => order.FulfillmentCenter)
            .Include(order => order.Items).ThenInclude(line => line.InventoryItem)
            .OrderByDescending(order => order.CreatedAt).Select(order => ToOrderResponse(order))
            .ToListAsync(cancellationToken);

    public async Task<OrderResponse?> GetOrderAsync(int id, CancellationToken cancellationToken)
    {
        var order = await GetOrderQuery().SingleOrDefaultAsync(row => row.Id == id, cancellationToken);
        return order is null ? null : ToOrderResponse(order);
    }

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var requestedIds = request.Items.Select(line => line.InventoryItemId).ToArray();
        if (requestedIds.Distinct().Count() != requestedIds.Length)
            throw new ArgumentException("An inventory item can only appear once in an order.");
        var inventory = await dbContext.InventoryItems.Where(item => requestedIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, cancellationToken);
        foreach (var line in request.Items)
        {
            if (!inventory.TryGetValue(line.InventoryItemId, out var item))
                throw new ArgumentException($"Inventory item {line.InventoryItemId} was not found.");
            if (item.FulfillmentCenterId != request.FulfillmentCenterId)
                throw new ArgumentException("All order items must be stocked at the selected fulfillment center.");
            if (item.QuantityOnHand < line.Quantity)
                throw new InvalidOperationException($"Insufficient stock for SKU {item.Sku}.");
        }

        var now = DateTimeOffset.UtcNow;
        var order = new Order
        {
            CustomerName = request.CustomerName.Trim(), CustomerEmail = request.CustomerEmail.Trim(),
            FulfillmentCenterId = request.FulfillmentCenterId, Status = OrderStatus.Pending,
            CreatedAt = now, UpdatedAt = now,
            Items = request.Items.Select(line =>
            {
                var item = inventory[line.InventoryItemId];
                item.QuantityOnHand -= line.Quantity;
                item.UpdatedAt = now;
                return new OrderLine { InventoryItemId = item.Id, Quantity = line.Quantity, UnitPrice = item.UnitPrice };
            }).ToList()
        };

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        var saved = await GetOrderQuery().SingleAsync(row => row.Id == order.Id, cancellationToken);
        return ToOrderResponse(saved);
    }

    public async Task<OrderResponse?> UpdateOrderAsync(int id, UpdateOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await GetOrderQuery().SingleOrDefaultAsync(row => row.Id == id, cancellationToken);
        if (order is null)
            return null;
        if (order.Status == OrderStatus.Cancelled && request.Status != OrderStatus.Cancelled)
            throw new InvalidOperationException("A cancelled order cannot be reopened.");
        if (request.Status == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
        {
            foreach (var line in order.Items)
            {
                line.InventoryItem!.QuantityOnHand += line.Quantity;
                line.InventoryItem.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }
        order.CustomerName = request.CustomerName.Trim();
        order.CustomerEmail = request.CustomerEmail.Trim();
        order.Status = request.Status;
        order.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToOrderResponse(order);
    }

    private IQueryable<Order> GetOrderQuery() => dbContext.Orders.Include(order => order.FulfillmentCenter)
        .Include(order => order.Items).ThenInclude(line => line.InventoryItem);

    private static InventoryItemResponse ToInventoryResponse(InventoryItem item) => new(
        item.Id, item.Sku, item.Name, item.FulfillmentCenterId, item.FulfillmentCenter!.Code,
        item.QuantityOnHand, item.ReorderLevel, item.UnitPrice, item.UpdatedAt);

    private static OrderResponse ToOrderResponse(Order order) => new(
        order.Id, order.CustomerName, order.CustomerEmail, order.FulfillmentCenterId, order.FulfillmentCenter!.Code,
        order.Status, order.CreatedAt, order.UpdatedAt,
        order.Items.Select(line => new OrderLineResponse(line.InventoryItemId, line.InventoryItem!.Sku,
            line.InventoryItem.Name, line.Quantity, line.UnitPrice)).ToList());
}