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

    public async Task ConfirmTableReservation(ConfirmTableRequest request)
    {
        var expectedCode = _branchAuthCode.GetCode(request.BranchId);

        if (expectedCode != request.AuthCode) throw new Exception("Invalid authentication code.");

        if (_tableManager.GetCurrentReservation(request.BranchId, request.TableNumber)?.OrderId != request.OrderId) 
            throw new Exception("No active reservation found for this order.");

        await _tableManager.BindNextReservation(request.BranchId, request.TableNumber);
    }
}