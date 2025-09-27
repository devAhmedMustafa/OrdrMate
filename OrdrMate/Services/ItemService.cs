namespace OrdrMate.Services;

using OrdrMate.Repositories;
using OrdrMate.Models;
using OrdrMate.DTOs.Item;
using OrdrMate.Features.ItemAvailability;
using OrdrMate.Events;

public class ItemService(IItemRepo itemRepo, ItemAvailabilityService itemAvailabilityService)
{
    private readonly IItemRepo _itemRepo = itemRepo;
    private readonly ItemAvailabilityService _itemAvailabilityService = itemAvailabilityService;

    public async Task<ItemDto?> AddItem(AddItemDto item)
    {
        try
        {
            var newItem = new Item
            {
                Name = item.Name,
                Description = item.Description,
                ImageUrl = item.ImageUrl,
                Price = item.Price,
                CategoryName = item.Category,
                RestaurantId = item.RestaurantId,
                KitchenId = item.KitchenId,
                PreperationTime = item.PreparationTime
            };

            var addedItem = await _itemRepo.AddItem(newItem);
            if (addedItem is null) throw new Exception("Failed to add item");

            ItemEvents.ItemAdded(addedItem);

            if (addedItem == null)
            {
                throw new Exception("Failed to add item");
            }

            return new ItemDto
            {
                Id = addedItem.Id,
                Name = addedItem.Name,
                Description = addedItem.Description,
                ImageUrl = addedItem.ImageUrl,
                Price = addedItem.Price,
                Category = addedItem.CategoryName,
                PreparationTime = addedItem.PreperationTime,
                KitchenName = addedItem.Kitchen?.Name ?? string.Empty
            };

        }
        catch (Exception ex)
        {
            throw new Exception($"Error adding item: {ex.Message}");
        }
    }

    public async Task<ItemDto?> GetItem(string id, string? branchId = null)
    {
        var item = await _itemRepo.GetItem(id);

        if (item == null)
        {
            throw new Exception("Item not found");
        }

        if (branchId != null)
        {
            throw new NotImplementedException("Item availability by branch is not implemented yet");
        }
        
        var isAvailable = branchId == null || await _itemAvailabilityService.IsItemAvailable(id, branchId);
        

        return new ItemDto
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            ImageUrl = item.ImageUrl,
            Price = item.Price,
            Category = item.CategoryName,
            PreparationTime = item.PreperationTime,
            KitchenName = item.Kitchen?.Name ?? string.Empty,
            KitchenId = item.Kitchen?.Id,
            Priority = item.Priority,
            Tags = item.Tags,
            IsAvailable = isAvailable
        };
    }

    public async Task<IEnumerable<Item>> GetAllItems()
    {
        return await _itemRepo.GetAllItems();
    }

    public async Task<IEnumerable<ItemDto>> GetItemsByRestaurantId(string restaurantId)
    {
        var items = await _itemRepo.GetItemsByRestaurantId(restaurantId);

        return items.Select(item => new ItemDto
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            ImageUrl = item.ImageUrl,
            Price = item.Price,
            Category = item.CategoryName,
            PreparationTime = item.PreperationTime,
            KitchenId = item.Kitchen?.Id,
            Priority = item.Priority,
            Tags = item.Tags,
            KitchenName = item.Kitchen?.Name ?? string.Empty
        });
    }

    public async Task<ItemDto?> UpdateItem(string id, UpdateItemDto updatedItem)
    {
        var existingItem = await _itemRepo.GetItem(id);
        if (existingItem == null)
        {
            throw new Exception("Item not found");
        }

        existingItem.Name = updatedItem.Name;
        existingItem.Description = updatedItem.Description;
        existingItem.ImageUrl = updatedItem.ImageUrl;
        existingItem.Price = updatedItem.Price;
        existingItem.CategoryName = updatedItem.Category;
        existingItem.KitchenId = updatedItem.KitchenId;
        existingItem.PreperationTime = updatedItem.PreparationTime;
        existingItem.Priority = updatedItem.Priority;
        existingItem.Tags = updatedItem.Tags;

        var updated = await _itemRepo.UpdateItem(existingItem);

        if (updated == null)
        {
            throw new Exception("Failed to update item");
        }

        return new ItemDto
        {
            Id = updated.Id,
            Name = updated.Name,
            Description = updated.Description,
            ImageUrl = updated.ImageUrl,
            Price = updated.Price,
            Category = updated.CategoryName,
            PreparationTime = updated.PreperationTime,
            KitchenName = updated.Kitchen?.Name ?? string.Empty,
            Priority = updated.Priority,
            Tags = updated.Tags
        };

    }

    public async Task<bool> DeleteItem(string id)
    {
        return await _itemRepo.DeleteItem(id);
    }

    public async Task<ItemAuthInfo> GetItemAuthInfo(string id)
    {
        var item = await _itemRepo.GetItem(id);

        if (item == null)
        {
            throw new Exception("Item not found");
        }

        return new ItemAuthInfo
        {
            Id = item.Id,
            RestaurantId = item.RestaurantId,
        };
    }

    
    public async Task<IEnumerable<ItemDto>> GetAvailableItems(string branchId)
    {
        var items = await _itemAvailabilityService.GetAllItemAvailabilities(branchId);
        return items;
    }
}