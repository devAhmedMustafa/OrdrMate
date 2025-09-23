using OrdrMate.DTOs.Order;
using OrdrMate.Models;

namespace OrdrMate.Mappers.Orders;

public static class OrdersDtoMapper
{
    public static OrderDto ToDto(this Order order)
    {
        if (order == null) throw new ArgumentNullException(nameof(order));

        return new OrderDto
        {
            OrderId = order.Id,
            RestaurantName = order.Branch?.Restaurant?.Name ?? "Unknown",
            CustomerId = order.CustomerId,
            Customer = order.Customer?.Username ?? "Unknown",
            OrderType = order.OrderType.ToString(),
            OrderItems = order.OrderItems?.Select(oi => new OrderItemDto
            {
                ItemId = oi.ItemId,
                Quantity = oi.Quantity,
                Price = oi.Price
            }).ToArray(),
            PaymentMethod = order.Payment?.PaymentMethod ?? "Unpaid",
            OrderDate = order.OrderDate,
            OrderStatus = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            BranchId = order.BranchId,
            IsPaid = order.IsPaid,
            OrderNumber = order.Takeaway?.OrderNumber,
            TableNumber = order.TableReservation?.TableNumber
        };
    }

    public static OrderDto ToDto(this IEnumerable<Order> orders)
    {
        if (orders == null) throw new ArgumentNullException(nameof(orders));

        var orderList = orders.Select(o => o.ToDto()).ToList();
        return new OrderDto
        {
            OrderId = orders.First().Id,
            RestaurantName = orders.First().Branch?.Restaurant?.Name ?? "Unknown",
            CustomerId = orders.First().CustomerId,
            Customer = orders.First().Customer?.Username ?? "Unknown",
            OrderType = orders.First().OrderType.ToString(),
            OrderItems = orders.First().OrderItems?.Select(oi => new OrderItemDto
            {
                ItemId = oi.ItemId,
                Quantity = oi.Quantity,
                Price = oi.Price
            }).ToArray(),
            PaymentMethod = orders.First().Payment?.PaymentMethod ?? "Unpaid",
            OrderDate = orders.First().OrderDate,
            OrderStatus = "Unified",
            TotalAmount = orders.Sum(o => o.TotalAmount),
            BranchId = orders.First().BranchId,
            IsPaid = orders.First().IsPaid,
            TableNumber = orders.First().TableReservation?.TableNumber
        };
    }
}