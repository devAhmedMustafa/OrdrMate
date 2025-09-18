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
            return await _repository.IsItemAvailable(itemId, branchId);
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
            Category = ia.Item.CategoryName,
            IsAvailable = ia.IsAvailable,
            Priority = ia.Item.Priority,
            Tags = ia.Item.Tags,
            Brand = ia.Item.Brand
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

    public async Task UpdateItemQuantity(UpdateItemQuantityDto data)
    {

        if (data.Quantity < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(data.Quantity));

        await _repository.UpdateItemQuantity(data.ItemId, data.BranchId, data.Quantity);
    }
}