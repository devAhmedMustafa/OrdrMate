using OrdrMate.DTOs.Item;

namespace OrdrMate.Features.ItemAvailability;

public class ItemAvailabilityService
{
    private readonly ItemAvailabilityRepository _repository;

    public ItemAvailabilityService(ItemAvailabilityRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> IsItemAvailabile(string itemId, string branchId)
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
            IsAvailable = ia.IsAvailable
        });
    }
}