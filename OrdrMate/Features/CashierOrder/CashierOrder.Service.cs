using OrdrMate.Services;
using OrdrMate.DTOs.Order;
using OrdrMate.Enums;
using OrdrMate.Models;
using OrdrMate.Repositories;
using OrdrMate.Features.BranchAttendance;

namespace OrdrMate.Features.CashierOrder;

public class CashierOrderService
{
    private readonly OrderService _orderService;
    private readonly BranchAttendanceService _branchAttendanceService;
    private readonly IOrderRepo _orderRepo;

    public CashierOrderService(OrderService orderService, BranchAttendanceService branchAttendanceService, IOrderRepo orderRepo)
    {
        _orderService = orderService;
        _branchAttendanceService = branchAttendanceService;
        _orderRepo = orderRepo;
    }

    public async Task<OrderIntentDto> CreateOrderForCashier(PlaceOrderDto placeOrderDto)
    {
        var orderIntent = await _orderService.CreateOrderIntent(placeOrderDto);

        switch (placeOrderDto.OrderType)
        {
            case OrderType.DineIn:

                int tableNumber = placeOrderDto.TableNumber ?? throw new ArgumentNullException(nameof(placeOrderDto.TableNumber), "Table number is required for DineIn orders.");

                await _branchAttendanceService.DirectTableSeating(placeOrderDto.BranchId, tableNumber);

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return orderIntent;
    }
       public async Task<Order> CreateGroupedOrderForTable(string tableId, List<OrderItem> items, string cashierId)
    {
        var order = new Order
        {
            TableReservationId = tableId,
            OrderItems = items,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            };

        return await _orderRepo.CreateOrder(order);
    }
}
