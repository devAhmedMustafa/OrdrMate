namespace OrdrMate.Services;

using OrdrMate.Repositories;
using OrdrMate.Models;
using OrdrMate.DTOs.Item;
using OrdrMate.Features.ItemAvailability;
using OrdrMate.Events;
using OrdrMate.Features.Storage;

public class ItemService(IItemRepo itemRepo, ItemAvailabilityService itemAvailabilityService, IStorageService storageService)
{
    private readonly IItemRepo _itemRepo = itemRepo;
    private readonly IStorageService _storageService = storageService;
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
                Category = item.Category,
                SubCategory = item.SubCategory,
                StoreId = item.StoreId,
                Brand = item.Brand,
                Priority = item.Priority,
                Tags = item.Tags
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
                Category = addedItem.Category,
                SubCategory = addedItem.SubCategory,
                Brand = addedItem.Brand,
                Priority = addedItem.Priority,
                Tags = addedItem.Tags
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
            ImageUrl = _storageService.GetDownloadPresignedUrl(item.ImageUrl).Data?.FileUrl,
            Price = item.Price,
            Category = item.Category,
            SubCategory = item.SubCategory,
            Brand = item.Brand,
            Priority = item.Priority,
            Tags = item.Tags
        };
    }

    public async Task<IEnumerable<Item>> GetAllItems()
    {
        return await _itemRepo.GetAllItems();
    }

    public async Task<IEnumerable<ItemDto>> GetItemsByStoreId(string storeId)
    {
        var items = await _itemRepo.GetItemsByStoreId(storeId);

        return items.Select(item => new ItemDto
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            ImageUrl = item.ImageUrl,
            Price = item.Price,
            Category = item.Category,
            SubCategory = item.SubCategory,
            Priority = item.Priority,
            Tags = item.Tags,
            Brand = item.Brand
        });
    }

    public async Task<ItemDto?> UpdateItem(string id, UpdateItemDto updatedItem)
    {
        var existingItem = await _itemRepo.GetItem(id);
        if (existingItem == null)
        {
            throw new Exception("Item not found");
        }

        if (updatedItem.Name is not null)
            existingItem.Name = updatedItem.Name;
        if (updatedItem.Description is not null)
            existingItem.Description = updatedItem.Description;
        if (updatedItem.ImageUrl is not null)
            existingItem.ImageUrl = updatedItem.ImageUrl;
        if (updatedItem.Price is not null)
            existingItem.Price = updatedItem.Price.Value;
        if (updatedItem.Category is not null)
            existingItem.Category = updatedItem.Category;
        if (updatedItem.SubCategory is not null)
            existingItem.SubCategory = updatedItem.SubCategory;
        if (updatedItem.Priority is not null)
            existingItem.Priority = updatedItem.Priority.Value;
        if (updatedItem.Tags is not null)
            existingItem.Tags = updatedItem.Tags;
        if (updatedItem.Brand is not null)
            existingItem.Brand = updatedItem.Brand;



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
            Category = updated.Category,
            SubCategory = updated.SubCategory,
            Brand = updated.Brand,
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
            StoreId = item.StoreId,
        };
    }


    public async Task<IEnumerable<ItemDto>> GetAvailableItems(string branchId)
    {
        var items = await _itemAvailabilityService.GetAllItemAvailabilities(branchId);
        return items;
    }
    
    public async Task<IEnumerable<ItemDto>> GetItemsByCategory(string storeId, string category)
    {
        var items = await _itemRepo.GetItemsByCategory(storeId, category);
        return items.Select(item => new ItemDto
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            ImageUrl = item.ImageUrl,
            Price = item.Price,
            Category = item.Category,
            SubCategory = item.SubCategory,
            Priority = item.Priority,
            Tags = item.Tags,
            Brand = item.Brand
        });
    }
}