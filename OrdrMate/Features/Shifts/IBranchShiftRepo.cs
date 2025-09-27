namespace OrdrMate.Features.Shifts
{
    public interface IBranchShiftRepo
    {
        Task<BranchShift> StartShift(BranchShift shift);
        Task<BranchShift> UpdateShift(BranchShift shift);
        Task<BranchShift?> GetCurrentShiftByBranchId(string branchId);
    }
}
