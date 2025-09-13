using OrdrMate.Events;
using OrdrMate.Models;
using OrdrMate.Services;

namespace OrdrMate.Features.ItemAvailability;

public class ItemAvailabilityOrch
{

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private static bool _initialized = false;

    public ItemAvailabilityOrch(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;

        if (_initialized) return;

        Init();

        ItemEvents.OnItemAdded += HandleItemAdded;
        ItemEvents.OnItemUpdated += HandleItemUpdated;
        ItemEvents.OnItemDeleted += HandleItemDeleted;

        BranchEvents.BranchCreated += HandleBranchAdded;
    }

    private void Init()
    {
        var itemAvailabilityService = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<ItemAvailabilityService>();
        var branchService = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<BranchService>();
        var itemService = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<ItemService>();

        var items = itemService.GetAllItems().Result;
        foreach (var item in items)
        {
            var branches = branchService.GetRestaurantBranches(item.RestaurantId).Result;
            foreach (var branch in branches)
            {
                itemAvailabilityService.IsItemAvailabile(item.Id, branch.BranchId).Wait();
                Console.WriteLine($"Checked availability for Item {item.Id} in Branch {branch.BranchId}");
            }
        }

        _initialized = true;
    }

    private void HandleItemAdded(Item item)
    {
        var itemAvailabilityService = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<ItemAvailabilityService>();
        var branchService = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<BranchService>();

        var branches = branchService.GetRestaurantBranches(item.RestaurantId).Result;
        foreach (var branch in branches)
        {
            itemAvailabilityService.IsItemAvailabile(item.Id, branch.BranchId).Wait();
        }
    }
    
    private void HandleBranchAdded(Branch branch)
    {
        var itemAvailabilityService = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<ItemAvailabilityService>();
        var itemService = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<ItemService>();

        var items = itemService.GetItemsByRestaurantId(branch.RestaurantId).Result;
        foreach (var item in items)
        {
            itemAvailabilityService.IsItemAvailabile(item.Id, branch.Id).Wait();
        }
    }

    private void HandleItemUpdated(string itemId)
    {

    }

    private void HandleItemDeleted(string itemId)
    {
        
    }
}