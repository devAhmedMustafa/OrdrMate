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

                await _branchAttendanceService.DirectTableSeating(placeOrderDto.BranchId, placeOrderDto.TableNumber!.Value);

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return orderIntent;
    }
}
