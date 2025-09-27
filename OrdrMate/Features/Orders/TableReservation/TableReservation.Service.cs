using OrdrMate.DTOs.Item;
using OrdrMate.DTOs.Order;
using OrdrMate.Features.Customization;
using OrdrMate.Repositories;
using OrdrMate.Utils.Exceptions;

namespace OrdrMate.Features.Orders.TableReservation;

public class TableReservationService
{
    
    private readonly ITableRepo _tableRepo;
    private readonly IOrderRepo _orderRepo;
    private readonly UserCustomizationService _userCustomizationService;

    public TableReservationService(
        ITableRepo tableRepo,
        IOrderRepo orderRepo,
        UserCustomizationService userCustomizationService)
    {
        _tableRepo = tableRepo;
        _orderRepo = orderRepo;
        _userCustomizationService = userCustomizationService;
    }

    public async Task<PushToKitchenResponseDto> PushToKitchenAsync(string reservationId)
    {
        try
        {
            var orders = await _tableRepo.GetTableOrdersByReservationId(reservationId);
            if (orders == null || !orders.Any())
            {
                throw new NotFoundException("No orders found for the given reservation ID.");
            }

            var queuedOrders = orders.Where(o => o.Status == Enums.OrderStatus.Queued);
            var orderItemsDtoList = new List<OrderItemDto>();

            foreach (var order in queuedOrders)
            {
                if (order.Status != Enums.OrderStatus.Queued)
                {
                    continue;
                }

                await _orderRepo.SetOrderStatus(order.Id, Enums.OrderStatus.InProgress);
                orderItemsDtoList.AddRange(order.OrderItems!.Select(item => new OrderItemDto
                {
                    ItemId = item.ItemId,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    OrderId = order.Id,
                    Item = item.Item == null ? null : new ItemDto
                    {
                        Id = item.Item.Id,
                        Name = item.Item.Name,
                        Description = item.Item.Description,
                        Price = item.Item.Price,
                        ImageUrl = item.Item.ImageUrl,
                        PreparationTime = item.Item.PreperationTime,
                        KitchenName = "Not Important",
                        Category = item.Item.CategoryName
                    }
                }));
            }


            foreach (var item in orderItemsDtoList)
            {
                var userCustomization = await _userCustomizationService.GetUserCustomization(item.OrderId!, item.ItemId);
                item.Customizations = userCustomization?.CustomizationValues.ToDictionary(
                    kvp => kvp.Name,
                    kvp => kvp.Value?.ToString() ?? string.Empty
                );
            }

            return new PushToKitchenResponseDto
            {
                OrderItems = [.. orderItemsDtoList]
            };
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"Failed to push to kitchen: {ex.Message}");
        }
    }
}