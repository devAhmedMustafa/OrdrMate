using OrdrMate.DTOs.Item;
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
            PharmacyName = order.Branch?.Pharmacy?.Name ?? "Unknown Pharmacy",
            CustomerId = order.CustomerId,
            Customer = order.Customer?.Username ?? "Unknown",
            OrderType = order.OrderType.ToString(),
            OrderItems = order.OrderItems?.Select(oi => new OrderItemDto
            {
                ItemId = oi.ItemId,
                Quantity = oi.Quantity,
                Price = oi.Price,
                Item = oi.Item != null ? new ItemDto
                {
                    Id = oi.Item.Id,
                    Name = oi.Item.Name,
                    Description = oi.Item.Description,
                    Price = oi.Item.Price,
                    Category = oi.Item.Category,
                    ImageUrl = oi.Item.ImageUrl,
                    Brand = oi.Item.Brand,
                } : null,
            }).ToArray(),
            PaymentMethod = order.Payment?.PaymentMethod ?? "Unpaid",
            OrderDate = order.OrderDate,
            OrderStatus = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            BranchId = order.BranchId,
            IsPaid = order.IsPaid,
            OrderNumber = order.Takeaway?.OrderNumber,
        };
    }

    public static OrderDto ToDto(this IEnumerable<Order> orders)
    {
        if (orders == null) throw new ArgumentNullException(nameof(orders));

        var orderList = orders.Select(o => o.ToDto()).ToList();
        return new OrderDto
        {
            OrderId = orders.First().Id,
            PharmacyName = orders.First().Branch?.Pharmacy?.Name ?? "Unknown Pharmacy",
            CustomerId = orders.First().CustomerId,
            Customer = orders.First().Customer?.Username ?? "Unknown",
            OrderType = orders.First().OrderType.ToString(),
            OrderItems = orders.First().OrderItems?.Select(oi => new OrderItemDto
            {
                ItemId = oi.ItemId,
                Quantity = oi.Quantity,
                Price = oi.Price,
                Item = oi.Item != null ? new ItemDto
                {
                    Id = oi.Item.Id,
                    Name = oi.Item.Name,
                    Description = oi.Item.Description,
                    Price = oi.Item.Price,
                    Category = oi.Item.Category,
                    ImageUrl = oi.Item.ImageUrl,
                    Brand = oi.Item.Brand,
                } : null,
            }).ToArray(),
            PaymentMethod = orders.First().Payment?.PaymentMethod ?? "Unpaid",
            OrderDate = orders.First().OrderDate,
            OrderStatus = "Unified",
            TotalAmount = orders.Sum(o => o.TotalAmount),
            BranchId = orders.First().BranchId,
            IsPaid = orders.First().IsPaid,
        };
    }

    public static IEnumerable<OrderDto> ToDtoList(this IEnumerable<Order> orders)
    {
        if (orders == null) throw new ArgumentNullException(nameof(orders));

        return [.. orders.Select(o => o.ToDto())];
    }
}