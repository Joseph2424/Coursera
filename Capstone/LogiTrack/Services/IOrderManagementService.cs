using LogiTrack.Contracts;

namespace LogiTrack.Services;

public interface IOrderManagementService
{
    Task<IReadOnlyList<FulfillmentCenterResponse>> GetFulfillmentCentersAsync(CancellationToken cancellationToken);
    Task<FulfillmentCenterResponse> CreateFulfillmentCenterAsync(CreateFulfillmentCenterRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<InventoryItemResponse>> GetInventoryAsync(CancellationToken cancellationToken);
    Task<InventoryItemResponse?> GetInventoryItemAsync(int id, CancellationToken cancellationToken);
    Task<InventoryItemResponse> CreateInventoryItemAsync(InventoryItemRequest request, CancellationToken cancellationToken);
    Task<InventoryItemResponse?> UpdateInventoryItemAsync(int id, InventoryItemRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<OrderResponse>> GetOrdersAsync(CancellationToken cancellationToken);
    Task<OrderResponse?> GetOrderAsync(int id, CancellationToken cancellationToken);
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken);
    Task<OrderResponse?> UpdateOrderAsync(int id, UpdateOrderRequest request, CancellationToken cancellationToken);
}