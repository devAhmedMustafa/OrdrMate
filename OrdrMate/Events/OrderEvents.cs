using OrdrMate.Models;

namespace OrdrMate.Events;

public static class OrderEvents
{
    public static event Action<string, List<OrderItem>>? OrderPlaced;
    public static event Action<string>? OrderInProgress;
    public static event Action<string, string>? OrderCancelled;
    public static event Action<string>? OrderReady;
    public static event Action<string, string, string>? OrderCompleted;
    public static event Action<string, string, string>? ItemInQueue;

    public static void OnOrderPlaced(string branchId, List<OrderItem> orderItems)
    {
        OrderPlaced?.Invoke(branchId, orderItems);
    }

    public static void OnItemInQueue(string branchId, string orderId, string itemId)
    {
        ItemInQueue?.Invoke(branchId, orderId, itemId);
    }

    public static void OnOrderReady(string orderId)
    {
        OrderReady?.Invoke(orderId);
    }

    public static void OnOrderInProgress(string orderId)
    {
        OrderInProgress?.Invoke(orderId);
    }

    public static void OnOrderCancelled(string branchId, string orderId)
    {
        OrderCancelled?.Invoke(branchId, orderId);
    }
}