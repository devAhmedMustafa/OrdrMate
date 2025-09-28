namespace OrdrMate.Features.Shifts
{
    public interface IBranchShiftRepo
    {
        Task<BranchShift> StartShift(BranchShift shift);
        Task<BranchShift> UpdateShift(BranchShift shift);
        Task<BranchShift?> GetById(string shiftId);
        Task<BranchShift?> GetCurrentShiftByBranchId(string branchId);
        Task<IEnumerable<BranchShift>> GetAllShiftsByBranchId(string branchId);
        Task<BranchShift?> GetLastShiftByBranchId(string branchId);
    }
}
