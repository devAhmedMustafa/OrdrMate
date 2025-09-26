using OrdrMate.Models;
namespace OrdrMate.Features.Shifts
{
    public interface IBranchShiftRepo
    {
        Task<BranchShift> StartShift(string branchId, DateTime startTime);
        Task<BranchShift> EndShift(string branchId, DateTime endTime);
        Task<BranchShift> GetCurrentShiftByBranchId(string branchId);
    }
}
