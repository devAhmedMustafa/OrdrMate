using OrdrMate.DTOs.Order;
using OrdrMate.Managers;

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

        if (tableOrder?.OrderId != request.OrderId)
            throw new Exception($"No active reservation found for this order. request.OrderId: {request.OrderId}, tableOrder?.OrderId: {tableOrder?.OrderId}");

        var order = await _tableManager.BindNextReservation(request.BranchId, request.TableNumber);
        if (order == null) throw new Exception("Failed to bind reservation to order.");
        
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
            TableNumber = request.TableNumber
        }; 
    }

    public async Task<OrderDto?> DirectTableSeating(string branchId, int tableNumber)
    {

        var order = await _tableManager.BindNextReservation(branchId, tableNumber);
        if (order == null) throw new Exception("Failed to bind reservation to order.");

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
            TableNumber = tableNumber
        };
    }
    
    public string GetBranchAttendanceCode(string branchId)
    {
        return _branchAuthCode.GetCode(branchId);
    }
}