using Microsoft.EntityFrameworkCore;
using OrdrMate.Data;
namespace OrdrMate.Features.Shifts
{
    public class BranchShiftRepo : IBranchShiftRepo
    {
        private readonly OrdrMateDbContext _context;

        public BranchShiftRepo(OrdrMateDbContext context)
        {
            _context = context;
        }

        public async Task<BranchShift> StartShift(string branchId, DateTime startTime)
        {
            var shift = new BranchShift
            {
                BranchId = branchId,
                ShiftStartTime = startTime,
                Status = ShiftStatus.Started
            };

            _context.BranchShifts.Add(shift);
            await _context.SaveChangesAsync();

            return shift;
        }

        public async Task<BranchShift> EndShift(string branchId, DateTime endTime)
        {
            var currentShift = await GetCurrentShiftByBranchId(branchId);
            if (currentShift == null || currentShift.Status == ShiftStatus.Ended)
                return null;

            currentShift.ShiftEndTime = endTime;
            currentShift.Status = ShiftStatus.Ended;

            _context.BranchShifts.Update(currentShift);
            await _context.SaveChangesAsync();

            return currentShift;
        }

        public async Task<BranchShift> GetCurrentShiftByBranchId(string branchId)
        {
            return await _context.BranchShifts
                .Where(s => s.BranchId == branchId && s.Status == ShiftStatus.Started && !s.ShiftEndTime.HasValue)
                .FirstOrDefaultAsync();
        }
    }
}
