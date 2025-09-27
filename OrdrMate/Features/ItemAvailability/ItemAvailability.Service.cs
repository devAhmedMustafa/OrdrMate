using OrdrMate.DTOs.Item;

namespace OrdrMate.Features.ItemAvailability;

public class ItemAvailabilityService
{
    private readonly ItemAvailabilityRepository _repository;

    public ItemAvailabilityService(ItemAvailabilityRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> IsItemAvailable(string itemId, string branchId)
    {

        var instance = await _repository.GetItemAvailability(itemId, branchId);
        if (instance is null)
        {
            await _repository.AddItemAvailability(new ItemAvailability
            {
                ItemId = itemId,
                BranchId = branchId,
            });

            return true;
        }
        else
        {
            return await _repository.IsItemAvailabile(itemId, branchId);
        }

    }

    public async Task<IEnumerable<ItemDto>> GetAllItemAvailabilities(string branchId)
    {
        var itemAvailabilities = await _repository.GetAllItemAvailabilities(branchId);
        return itemAvailabilities.Select(ia => new ItemDto
        {
            Id = ia.ItemId,
            Name = ia.Item!.Name,
            Description = ia.Item.Description,
            ImageUrl = ia.Item.ImageUrl,
            Price = ia.Item.Price,
            PreparationTime = ia.Item.PreperationTime,
            Category = ia.Item.CategoryName,
            KitchenName = ia.Item.Kitchen!.Name,
            KitchenId = ia.Item.KitchenId,
            IsAvailable = ia.IsAvailable,
            Priority = ia.Item.Priority,
            Tags = ia.Item.Tags
        });
    }

    public async Task<bool> ToggleItemAvailability(string itemId, string branchId)
    {
        var instance = await _repository.GetItemAvailability(itemId, branchId);
        if (instance is null)
        {
            instance = await _repository.AddItemAvailability(new ItemAvailability
            {
                ItemId = itemId,
                BranchId = branchId,
                IsAvailable = false
            });
        }
        else
        {
            instance.IsAvailable = !instance.IsAvailable;
            instance = await _repository.UpdateItemAvailability(instance);
        }

        return instance.IsAvailable;
    }

    public async Task<ItemAvailability?> GetItemAvailability(string itemId, string branchId)
    {
        try
        {
            return await _repository.GetItemAvailability(itemId, branchId);
        }
        catch (Exception)
        {
            return null;
        }
    }
}