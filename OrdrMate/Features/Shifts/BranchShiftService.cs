using OrdrMate.Repositories;
using OrdrMate.Models;
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

        public async Task<BranchShift> StartShiftAsync(string branchId, DateTime startTime)
        {
            return await _branchShiftRepo.StartShift(branchId, startTime);
        }

        public async Task<BranchShift> EndShiftAsync(string branchId, DateTime endTime)
        {
            return await _branchShiftRepo.EndShift(branchId, endTime);
        }
        public async Task<IEnumerable<Order>> GetOrdersForShift(int shiftId)
        {
            return await _orderRepo.GetOrdersByShiftId(shiftId);
        }
    }
}
