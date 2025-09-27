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
            RestaurantName = order.Branch?.Restaurant?.Name ?? "Unknown",
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
                    Category = oi.Item.CategoryName,
                    ImageUrl = oi.Item.ImageUrl,
                    PreparationTime = oi.Item.PreperationTime,
                    KitchenName = oi.Item.Kitchen?.Name ?? "Main",
                } : null,
            }).ToArray(),
            PaymentMethod = order.Payment?.PaymentMethod ?? "Unpaid",
            PaymentProvider = order.Payment?.Provider ?? "Unpaid",
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

        return new OrderDto
        {
            OrderId = orders.First().Id,
            RestaurantName = orders.First().Branch?.Restaurant?.Name ?? "Unknown",
            CustomerId = orders.First().CustomerId,
            Customer = orders.First().Customer?.Username ?? "Unknown",
            OrderType = orders.First().OrderType.ToString(),
            OrderItems = [.. orders
                .SelectMany(o => o.OrderItems ?? Enumerable.Empty<OrderItem>())
                .Select(oi => new OrderItemDto
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
                        Category = oi.Item.CategoryName,
                        ImageUrl = oi.Item.ImageUrl,
                        PreparationTime = oi.Item.PreperationTime,
                        KitchenName = oi.Item.Kitchen?.Name ?? "Main",
                    } : null,
                })],
            PaymentMethod = orders.First().Payment?.PaymentMethod ?? "Unpaid",
            PaymentProvider = orders.First().Payment?.Provider ?? "Unpaid",
            OrderDate = orders.First().OrderDate,
            OrderStatus = orders.Select(o =>
            {
                if (o.Status == Enums.OrderStatus.Cancelled) return Enums.OrderStatus.Ready;
                return o.Status;
            }).Min().ToString(),
            TotalAmount = orders.Sum(o => o.TotalAmount),
            BranchId = orders.First().BranchId,
            IsPaid = orders.All(o => o.IsPaid),
            TableNumber = orders.First().TableReservation?.TableNumber
        };
    }
}