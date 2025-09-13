namespace OrdrMate.Repositories;

using OrdrMate.Enums;
using OrdrMate.Models;

public interface IOrderRepo
{
    Task<OrderIntent> CreateOrderIntent(OrderIntent orderIntent);
    Task<OrderIntent?> GetOrderIntentById(string orderIntentId);
    Task<OrderIntent?> UpdateOrderIntentStatus(string orderIntentId, PaymentStatus status);
    Task<Order> CreateOrder(Order order);
    Task<Takeaway> CreateTakeawayOrder(Takeaway takeaway);
    Task<OrderItem> CreateOrderItem(OrderItem orderItem);
    Task<Order?> GetOrderById(string orderId);
    Task<Order> GetDetailedOrderById(string orderId);
    Task<Takeaway?> GetTakeawayById(string orderId);
    Task<IEnumerable<Takeaway>> GetTakeawaysByCustomerId(string customerId);
    Task<IEnumerable<Takeaway>> GetAllTakeawaysByBranchId(string branchId);
    Task<Order?> SetOrderPaidStatus(string orderId, bool isPaid);
    Task<Order?> SetOrderStatus(string orderId, OrderStatus status);
    Task<IEnumerable<Order>> GetReadyOrdersByBranchId(string branchId);
    Task<IEnumerable<Order>> GetAllOrdersByBranchId(string branchId);
    Task<IEnumerable<Order>> GetUnpaidOrdersByBranchId(string branchId);
    Task<IEnumerable<Order>> GetPaidOrdersOfBranch(string branchId);
}