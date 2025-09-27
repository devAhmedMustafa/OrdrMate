using OrdrMate.Repositories;
using OrdrMate.Utils.Exceptions;
using OrdrMate.DTOs.Order;
using OrdrMate.Mappers.Orders;
using System.Text;
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

        public async Task<BranchShift> GetCurrentShiftStatusAsync(string branchId)
        {
            try
            {
                var shift = await _branchShiftRepo.GetCurrentShiftByBranchId(branchId);
                if (shift == null)
                {
                    throw new NotFoundException("No ongoing shift found for the branch.");
                }

                return shift;
            }
            catch (OException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InternalServerException($"An error occurred while retrieving the shift status: {ex.Message}");
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
                Console.WriteLine($"[BranchShift]: Retrieved {orders.Count()} orders for shift {shiftId}.");
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

        public async Task<IEnumerable<BranchShift>> GetAllShiftsForBranchAsync(string branchId)
        {
            try
            {
                return await _branchShiftRepo.GetAllShiftsByBranchId(branchId);
            }
            catch (OException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InternalServerException($"An error occurred while retrieving shifts: {ex.Message}");
            }
        }

        public async Task<string> GetOrdersForShiftCsv(string shiftId)
        {
            try
            {
                var orders = await GetOrdersForShift(shiftId);

                var csvBuilder = new StringBuilder();
                csvBuilder.AppendLine("OrderId,TotalAmount,PaymentProvider,OrderDate");

                foreach (var order in orders)
                {
                    csvBuilder.AppendLine($"{order.OrderId},{order.TotalAmount},{order.PaymentProvider},{order.OrderDate:O}");
                }

                return csvBuilder.ToString();
            }
            catch (OException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InternalServerException($"An error occurred while generating CSV for orders: {ex.Message}");
            }
        }
        public async Task<ShiftEarningsResponse.Dto> GetEarningsForBranchAsync(string branchId)
        {
            try
            {
                var lastShift = await _branchShiftRepo.GetLastShiftByBranchId(branchId);
                if (lastShift == null)
                {
                    throw new NotFoundException("No shifts found for the branch.");
                }

                var orders = await _orderRepo.GetOrdersWithinShift(branchId, lastShift.ShiftStartTime, lastShift.ShiftEndTime ?? DateTime.UtcNow);
                var totalEarnings = orders.Sum(o => o.TotalAmount);
                var totalOrders = orders.Count();

                return new ShiftEarningsResponse.Dto
                {
                    ShiftId = lastShift.Id,
                    BranchId = branchId,
                    StartTime = lastShift.ShiftStartTime,
                    EndTime = lastShift.ShiftEndTime,
                    TotalEarnings = totalEarnings,
                    TotalOrders = totalOrders
                };
            }
            catch (OException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InternalServerException($"An error occurred while calculating earnings: {ex.Message}");
            }
        }
    }

}
