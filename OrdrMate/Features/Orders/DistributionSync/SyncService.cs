using OrdrMate.Models;
using OrdrMate.Repositories;

namespace OrdrMate.Features.Orders.DistributionSync;

public class SyncService
{
    private readonly IOrderRepo _orderRepo;
    private readonly ITableRepo _tableRepo;

    public SyncService(IOrderRepo orderRepo, ITableRepo tableRepo)
    {
        _orderRepo = orderRepo;
        _tableRepo = tableRepo;
    }

    public async Task PushOrder(PushOrderRequest dto)
    {
        try
        {
            Order order = dto.Order;

            foreach (var itemDto in dto.OrderItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ItemId = itemDto.ItemId,
                    Quantity = itemDto.Quantity,
                    Price = itemDto.Price
                };
                await _orderRepo.CreateOrderItem(orderItem);
            }

            Models.TableReservation? tableReservation = null;

            if (dto.TableNumber.HasValue)
            {
                tableReservation = await _tableRepo.CreateTableReservation(new Models.TableReservation
                {
                    BranchId = order.BranchId,
                    CustomerId = order.CustomerId,
                    TableNumber = dto.TableNumber.Value,
                    ReservationTime = order.OrderDate
                });
            }

            order.TableReservationId = tableReservation?.ReservationId;
            await _orderRepo.CreateOrder(order);
        }
        catch (Exception)
        {
            throw;
        }
    }
}