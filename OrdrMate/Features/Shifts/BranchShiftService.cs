namespace OrdrMate.Features.Shifts
{
    public class BranchShiftService
    {
        private readonly IBranchShiftRepo _branchShiftRepo;

        public BranchShiftService(IBranchShiftRepo branchShiftRepo)
        {
            _branchShiftRepo = branchShiftRepo;
        }

        public async Task<BranchShift> StartShiftAsync(string branchId, DateTime startTime)
        {
            return await _branchShiftRepo.StartShift(branchId, startTime);
        }

        public async Task<BranchShift> EndShiftAsync(string branchId, DateTime endTime)
        {
            return await _branchShiftRepo.EndShift(branchId, endTime);
        }
    }
}
