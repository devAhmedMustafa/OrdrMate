using Microsoft.EntityFrameworkCore;
using OrdrMate.Data;
using OrdrMate.Utils.Exceptions;
namespace OrdrMate.Features.Shifts
{
    public class BranchShiftRepo : IBranchShiftRepo
    {
        private readonly OrdrMateDbContext _context;

        public BranchShiftRepo(OrdrMateDbContext context)
        {
            _context = context;
        }

        public async Task<BranchShift> StartShift(BranchShift shift)
        {
            try
            {
                _context.BranchShifts.Add(shift);
                await _context.SaveChangesAsync();
                return shift;
            }
            catch (Exception ex)
            {
                throw new InternalServerException($"Error starting shift: {ex.Message}");
            }
        }

        public async Task<BranchShift> UpdateShift(BranchShift shift)
        {
            var entry = _context.BranchShifts.Update(shift);
            await _context.SaveChangesAsync();

            return entry.Entity;
        }

        public async Task<BranchShift?> GetCurrentShiftByBranchId(string branchId)
        {
            return await _context.BranchShifts
                .Where(s => s.BranchId == branchId && s.Status == ShiftStatus.Started && !s.ShiftEndTime.HasValue)
                .FirstOrDefaultAsync();
        }
    }
}
