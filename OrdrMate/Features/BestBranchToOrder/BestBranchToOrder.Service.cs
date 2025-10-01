using OrdrMate.DTOs.Order;
using OrdrMate.Features.ItemAvailability;
using OrdrMate.Models;
using OrdrMate.Repositories;
using OrdrMate.Utils;

namespace OrdrMate.Features.BestBranchToOrder;

public class BestBranchToOrderService
{

    private readonly GeoMaps _geoMaps;
    private readonly IBranchRepo _branchRepo;
    private readonly ItemAvailabilityService _itemAvailabilityService;

    public BestBranchToOrderService(
        GeoMaps geoMaps,
        IBranchRepo branchRepo,
        ItemAvailabilityService itemAvailabilityService
        )
    {
        _geoMaps = geoMaps;
        _branchRepo = branchRepo;
        _itemAvailabilityService = itemAvailabilityService;
    }

    public async Task<string> FindBestBranchToOrder(PlaceOrderDto orderDto)
    {
        var branches = await _branchRepo.GetStoreBranches(orderDto.StoreId);
        branches = FilterBranchesByWorkingHours(branches);
        branches = await FilterBranchesWithAllItemsAvailable(orderDto, branches);
        var bestBranch = FindClosestBranch(orderDto.Latitude, orderDto.Longitude, branches);
        return bestBranch.Id;
    }

    private List<Branch> FilterBranchesByWorkingHours(IEnumerable<Branch> branches)
    {

        try
        {
            var currentTime = DateTime.UtcNow.TimeOfDay;
            var currentDayOfWeek = DateTime.UtcNow.DayOfWeek;

            var openBranches = branches.Where(branch =>
            {
                return TimeService.CheckWithinTimeInterval(
                    branch.StartWorkingHour,
                    branch.EndWorkingHour,
                    branch.WorkingDays);
            }).ToList();
            
            if (openBranches.Count == 0)
                throw new Exception("No branches are currently open");

            return openBranches;
        }
        catch (Exception ex)
        {
            throw new Exception("Error filtering branches by working hours", ex);
        }
    }

    private async Task<List<Branch>> FilterBranchesWithAllItemsAvailable(PlaceOrderDto orderDto, IEnumerable<Branch> branches)
    {
        try
        {
            var availableBranches = new List<Branch>();

            foreach (var branch in branches)
            {
                bool allItemsAvailable = true;

                foreach (var item in orderDto.Items)
                {
                    bool isAvailable = await _itemAvailabilityService.IsItemAvailable(item.ItemId, branch.Id);
                    if (!isAvailable)
                    {
                        allItemsAvailable = false;
                        break;
                    }
                }

                if (allItemsAvailable)
                {
                    availableBranches.Add(branch);
                }
            }

            if (availableBranches.Count == 0)
                throw new Exception("No branches have all items available");

            return availableBranches;
        }
        catch (Exception ex)
        {
            throw new Exception("Error filtering branches by item availability", ex);
        }
    }

    private Branch FindClosestBranch(double latitude, double longitude, IEnumerable<Branch> branches)
    {
        try
        {
            var sortedBranches = branches.OrderBy(b => _geoMaps.CalculateDistance(latitude, longitude, b.Latitude, b.Longitude)).ToList();

            if (sortedBranches.Count == 0)
                throw new Exception("No branches available");

            return sortedBranches.FirstOrDefault() ?? throw new Exception("No branches available");
        }
        catch (Exception ex)
        {
            throw new Exception("Error finding the closest branch", ex);
        }
    }

}