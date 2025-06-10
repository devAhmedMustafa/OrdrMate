namespace OrdrMate.Services;

using OrdrMate.Repositories;
using OrdrMate.Models;
using OrdrMate.DTOs.Item;

public class ItemService(IItemRepo itemRepo)
{
    private readonly IItemRepo _itemRepo = itemRepo;

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

    public async Task<ItemDto?> GetItem(string id)
    {
        var item = await _itemRepo.GetItem(id);

        if (item == null)
        {
            throw new Exception("Item not found");
        }

        return new ItemDto
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            ImageUrl = item.ImageUrl,
            Price = item.Price,
            Category = item.CategoryName,
            PreparationTime = item.PreperationTime,
            KitchenName = item.Kitchen?.Name ?? string.Empty
        };
    }

    public async Task<IEnumerable<Item>> GetItems()
    {
        return await _itemRepo.GetItems();
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
            KitchenName = item.Kitchen?.Name ?? string.Empty
        });
    }

    public async Task<ItemDto?> UpdateItem(string id, UpdateItemDto updatedItem)
    {
        Item item = new()
        {
            Name = updatedItem.Name,
            Description = updatedItem.Description,
            ImageUrl = updatedItem.ImageUrl,
            Price = updatedItem.Price,
            CategoryName = updatedItem.Category,
            KitchenId = updatedItem.KitchenId,
            PreperationTime = updatedItem.PreparationTime
        };

        var updated = await _itemRepo.UpdateItem(id, item);

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
            KitchenName = updated.Kitchen?.Name ?? string.Empty
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
        var items = await _itemRepo.GetAvailableItemsByBranchId(branchId);
        return items.Select(i => new ItemDto
        {
            Id = i.Id,
            Name = i.Name,
            Price = i.Price,
            Description = i.Description,
            Category = i.CategoryName,
            PreparationTime = i.PreperationTime,
            KitchenName = i.Kitchen?.Name ?? "Unknown",
            ImageUrl = i.ImageUrl,
        });
    }
}