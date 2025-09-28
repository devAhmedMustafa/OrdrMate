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

        public async Task<BranchShift?> GetById(string shiftId)
        {
            try
            {
                return await _context.BranchShifts.FindAsync(shiftId);
            }
            catch (Exception ex)
            {
                throw new InternalServerException($"Error retrieving shift: {ex.Message}");
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
            try
            {
                return await _context.BranchShifts
                    .Where(s => s.BranchId == branchId && s.Status == ShiftStatus.Started && !s.ShiftEndTime.HasValue)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new InternalServerException($"Error retrieving current shift: {ex.Message}");
            }
        }

        public async Task<IEnumerable<BranchShift>> GetAllShiftsByBranchId(string branchId)
        {
            try
            {
                return await _context.BranchShifts
                    .Where(s => s.BranchId == branchId)
                    .OrderByDescending(s => s.ShiftStartTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InternalServerException($"Error retrieving shifts: {ex.Message}");
            }
        }

        public async Task<BranchShift?> GetLastShiftByBranchId(string branchId)
        {
            try
            {
                return await _context.BranchShifts
                    .Where(s => s.BranchId == branchId)
                    .OrderByDescending(s => s.ShiftStartTime)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new InternalServerException($"Error retrieving last shift: {ex.Message}");
            }
        }
    }
}
