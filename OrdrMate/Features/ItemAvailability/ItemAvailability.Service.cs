using OrdrMate.DTOs.Item;
using OrdrMate.Features.Storage;

namespace OrdrMate.Features.ItemAvailability;

public class ItemAvailabilityService
{
    private readonly ItemAvailabilityRepository _repository;
    private readonly IStorageService _storageService;

    public ItemAvailabilityService(ItemAvailabilityRepository repository, IStorageService storageService)
    {
        _repository = repository;
        _storageService = storageService;
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
            ImageUrl = _storageService.GetDownloadPresignedUrl(ia.Item.ImageUrl).Data?.FileUrl,
            Price = ia.Item.Price,
            Category = ia.Item.Category,
            SubCategory = ia.Item.SubCategory,
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

    public async Task<IEnumerable<ItemAvailabilityResponse>> GetItemAvailabilities(string branchId)
    {
        try
        {
            var itemAvailabilities = await _repository.GetAllItemAvailabilities(branchId);
            return itemAvailabilities.Select(ia => new ItemAvailabilityResponse
            {
                ItemId = ia.ItemId,
                BranchId = ia.BranchId,
                IsAvailable = ia.IsAvailable,
                Stock = ia.AvailableQuantity
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while fetching item availabilities: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateItemQuantity(UpdateItemQuantityDto data)
    {

        if (data.Quantity < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(data.Quantity));

        await _repository.UpdateItemQuantity(data.ItemId, data.BranchId, data.Quantity);
    }
    
    public async Task DecreaseItemQuantity(string itemId, string branchId, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        var itemAvailability = await _repository.GetItemAvailability(itemId, branchId);
        if (itemAvailability == null)
            throw new InvalidOperationException("Item availability record not found");

        if (itemAvailability.AvailableQuantity < quantity)
            throw new InvalidOperationException("Insufficient stock available");

        itemAvailability.AvailableQuantity -= quantity;
        await _repository.UpdateItemAvailability(itemAvailability);
    }
}