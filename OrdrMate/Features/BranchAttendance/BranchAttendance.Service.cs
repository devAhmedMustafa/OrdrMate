using OrdrMate.DTOs.Order;
using OrdrMate.Managers;
using OrdrMate.Mappers.Orders;

namespace OrdrMate.Features.BranchAttendance;

public class BranchAttendanceService
{
    private readonly BranchAuthCode _branchAuthCode;
    private readonly TableManager _tableManager;

    public BranchAttendanceService(BranchAuthCode branchAuthCode, TableManager tableManager)
    {
        _branchAuthCode = branchAuthCode;
        _tableManager = tableManager;
    }

    public async Task<OrderDto?> ConfirmTableReservation(ConfirmTableRequest request)
    {
        var expectedCode = _branchAuthCode.GetCode(request.BranchId);

        if (expectedCode != request.AuthCode) throw new Exception($"Invalid authentication code. Expected {expectedCode}, got {request.AuthCode}");

        var tableOrder = _tableManager.GetCurrentReservation(request.BranchId, request.TableNumber);

        var orders = await _tableManager.BindNextReservation(request.BranchId, request.TableNumber);
        if (orders == null) throw new Exception("Failed to bind reservation to order.");
        
        return OrdersDtoMapper.ToDto(orders);
    }

    public async Task<OrderDto?> DirectTableSeating(string branchId, int tableNumber)
    {

        var orders = await _tableManager.BindNextReservation(branchId, tableNumber);
        if (orders == null) throw new Exception("Failed to bind reservation to order.");

        return OrdersDtoMapper.ToDto(orders);
    }
    
    public string GetBranchAttendanceCode(string branchId)
    {
        return _branchAuthCode.GetCode(branchId);
    }
}