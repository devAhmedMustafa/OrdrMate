using OrdrMate.Repositories;
using OrdrMate.Models;
using OrdrMate.Utils.Exceptions;
using OrdrMate.DTOs.Order;
using OrdrMate.Mappers.Orders;
namespace OrdrMate.Features.Shifts
{
    public class BranchShiftService
    {
        private readonly IBranchShiftRepo _branchShiftRepo;
        private readonly IOrderRepo _orderRepo;

        public BranchShiftService(IBranchShiftRepo branchShiftRepo, IOrderRepo orderRepo)
        {
            _branchShiftRepo = branchShiftRepo;
            _orderRepo = orderRepo;
        }

        public async Task<BranchShift> StartShiftAsync(string branchId)
        {
            try
            {
                var existingShift = await _branchShiftRepo.GetCurrentShiftByBranchId(branchId);

                if (existingShift != null)
                {
                    throw new BadRequestException("A shift is already in progress for this branch.");
                }

                var shift = new BranchShift
                {
                    BranchId = branchId,
                    Status = ShiftStatus.Started
                };

                return await _branchShiftRepo.StartShift(shift);
            }
            catch (OException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InternalServerException($"An error occurred while starting the shift: {ex.Message}");
            }
        }

        public async Task<BranchShift> EndShiftAsync(string branchId)
        {
            try
            {
                var existingShift = await _branchShiftRepo.GetCurrentShiftByBranchId(branchId);

                if (existingShift == null || existingShift.Status != ShiftStatus.Started)
                {
                    throw new BadRequestException("No active shift found for this branch.");
                }

                existingShift.ShiftEndTime = DateTime.UtcNow;
                existingShift.Status = ShiftStatus.Ended;

                return await _branchShiftRepo.UpdateShift(existingShift);
            }
            catch (OException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InternalServerException($"An error occurred while ending the shift: {ex.Message}");
            }
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersForShift(string shiftId)
        {
            try
            {
                var shift = await _branchShiftRepo.GetById(shiftId);

                if (shift == null)
                {
                    throw new NotFoundException("Shift not found.");
                }

                var orders = await _orderRepo.GetOrdersWithinShift(shift.BranchId, shift.ShiftStartTime, shift.ShiftEndTime ?? DateTime.UtcNow);
                return orders.Select(OrdersDtoMapper.ToDto);
            }
            catch (OException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InternalServerException($"An error occurred while retrieving orders for the shift: {ex.Message}");
            }
        }
    }
}
