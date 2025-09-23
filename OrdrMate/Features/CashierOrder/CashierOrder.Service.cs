using OrdrMate.Services;
using OrdrMate.DTOs.Order;
using OrdrMate.Enums;
using OrdrMate.Features.BranchAttendance;

namespace OrdrMate.Features.CashierOrder;

public class CashierOrderService
{
    private readonly OrderService _orderService;
    private readonly BranchAttendanceService _branchAttendanceService;

    public CashierOrderService(OrderService orderService, BranchAttendanceService branchAttendanceService)
    {
        _orderService = orderService;
        _branchAttendanceService = branchAttendanceService;
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
}
