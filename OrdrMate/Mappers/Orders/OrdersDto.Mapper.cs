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
            StoreName = order.Branch?.Store?.Name ?? "Unknown Store",
            CustomerId = order.CustomerId,
            Customer = order.CustomerName ?? "Unknown",
            CustomerPhone = order.CustomerPhone ?? "Unknown Phone",
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
            DeliveryDetails = order.Delivery != null ? new DeliveryDetailsDto
            {
                Address = order.Delivery.Address,
                Latitude = order.Delivery.Latitude,
                Longitude = order.Delivery.Longitude,
            } : null,
        };
    }

    public static IEnumerable<OrderDto> ToDtoList(this IEnumerable<Order> orders)
    {
        if (orders == null) throw new ArgumentNullException(nameof(orders));

        return [.. orders.Select(o => o.ToDto())];
    }
}