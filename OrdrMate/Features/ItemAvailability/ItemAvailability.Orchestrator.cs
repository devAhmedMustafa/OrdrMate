using OrdrMate.Events;
using OrdrMate.Models;
using OrdrMate.Services;

namespace OrdrMate.Features.ItemAvailability;

public class ItemAvailabilityOrchestrator : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private static bool _initialized = false;

    public ItemAvailabilityOrchestrator(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_initialized) return Task.CompletedTask;

        _ = InitializeAsync(cancellationToken);

        ItemEvents.OnItemAdded += HandleItemAdded;
        ItemEvents.OnItemUpdated += HandleItemUpdated;
        ItemEvents.OnItemDeleted += HandleItemDeleted;
        BranchEvents.BranchCreated += HandleBranchAdded;

        _initialized = true;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        ItemEvents.OnItemAdded -= HandleItemAdded;
        ItemEvents.OnItemUpdated -= HandleItemUpdated;
        ItemEvents.OnItemDeleted -= HandleItemDeleted;
        BranchEvents.BranchCreated -= HandleBranchAdded;

        return Task.CompletedTask;
    }

    private async Task InitializeAsync(CancellationToken _)
    {
        using var scope = _scopeFactory.CreateScope();
        var itemAvailabilityService = scope.ServiceProvider.GetRequiredService<ItemAvailabilityService>();
        var branchService = scope.ServiceProvider.GetRequiredService<BranchService>();
        var itemService = scope.ServiceProvider.GetRequiredService<ItemService>();

        var items = await itemService.GetAllItems();
        foreach (var item in items)
        {
            var branches = await branchService.GetPharmacyBranches(item.PharmacyId);
            foreach (var branch in branches)
            {
                await itemAvailabilityService.IsItemAvailable(item.Id, branch.BranchId);
            }
        }
    }

    private async void HandleItemAdded(Item item)
    {
        using var scope = _scopeFactory.CreateScope();
        var itemAvailabilityService = scope.ServiceProvider.GetRequiredService<ItemAvailabilityService>();
        var branchService = scope.ServiceProvider.GetRequiredService<BranchService>();

        var branches = await branchService.GetPharmacyBranches(item.PharmacyId);
        foreach (var branch in branches)
        {
            await itemAvailabilityService.IsItemAvailable(item.Id, branch.BranchId);
        }
    }

    private async void HandleBranchAdded(Branch branch)
    {
        using var scope = _scopeFactory.CreateScope();
        var itemAvailabilityService = scope.ServiceProvider.GetRequiredService<ItemAvailabilityService>();
        var itemService = scope.ServiceProvider.GetRequiredService<ItemService>();

        var items = await itemService.GetItemsByPharmacyId(branch.PharmacyId);
        foreach (var item in items)
        {
            await itemAvailabilityService.IsItemAvailable(item.Id, branch.Id);
        }
    }

    private void HandleItemUpdated(string itemId)
    {
        // TODO: implement
    }

    private void HandleItemDeleted(string itemId)
    {
        // TODO: implement
    }
}
